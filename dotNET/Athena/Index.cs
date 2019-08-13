using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Athena.Entities;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// Index files
    /// </summary>
    public class Index : Converter
    {
        const string ResourceLogFile = "Rigsarkiv.Athena.Resources.report.html";
        const string FileIndex = "fileIndex.xml";
        private XDocument _fileIndexXDocument = null;

        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public Index(LogManager logManager, string srcPath, string destPath, string destFolder,Report report) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Index";
            _report = report;
            using (Stream stream = _assembly.GetManifestResourceStream(string.Format(ResourcePrefix, FileIndex)))
            {
                _fileIndexXDocument = XDocument.Load(stream);
            }
        }

        /// <summary>
        /// start converter
        /// </summary>
        /// <returns></returns>
        public override bool Run()
        {
            var result = false;
            var message = string.Format("Start Indexing files at {0}", _destFolderPath);
            _log.Info(message);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
            if(EnsureReport() && IndexFiles()) { result = true; }
            message = result ? "End Indexing files" : "End Indexing files with errors";
            _log.Info(message);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
            return result;
        }

        /// <summary>
        /// flush and save report file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public bool Flush(string path, string name)
        {
            var result = true;
            try
            {
                _log.Info("Flush report");
                var json = new JavaScriptSerializer().Serialize(_report);
                string data = GetLogTemplate();
               File.WriteAllText(path, string.Format(data, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), name, json));
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("Failed to Flush log", ex);
            }
            return result;
        }

        private bool EnsureReport()
        {
            var result = (_report.Tables != null && _report.Tables.Count > 0);
            if(result)
            {
                _report.Tables.ForEach(mainTable => {
                    UpdateReport(mainTable);
                    if(mainTable.CodeList != null)
                    {
                        mainTable.CodeList.ForEach(table => {
                            UpdateReport(table);
                        });
                    }                    
                 });
            }
            else
            {
                var message = "Tables metadata Property is empty";
                _log.Info(message);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = message });
            }
            return result;
        }

        private void UpdateReport(Table table)
        {
            table.Columns.Where(c => c.Errors > 0).ToList().ForEach(c => {
                c.ErrorsRows.ForEach(i => {
                    var index = i.ToString();
                    if (!table.ErrorsRows.ContainsKey(index))
                    {
                        _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Update report table {0} with error row index: {1}", table.Name, index) });
                        table.ErrorsRows.Add(index, GetRow(table, i + 1));
                    }
                });
            });
        }

        private bool IndexFiles()
        {
            var result = true;
            try
            {
                var path = string.Format("{0}\\{1}", string.Format(IndicesPath, _destFolderPath), FileIndex);
                var files = Directory.GetFiles(_destFolderPath, "*.*", SearchOption.AllDirectories);
                files.Where(filePath => path != filePath).ToList().ForEach(filePath =>
                {
                    var relativePath = filePath.Substring(_destPath.Length + 1);
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Index file: {0}", relativePath) });
                    var folder = relativePath.Substring(0, relativePath.LastIndexOf("\\"));
                    var fileName = relativePath.Substring(relativePath.LastIndexOf("\\") + 1);
                    if (folder.StartsWith("\\")) { folder = folder.Substring(1); }
                    var fileNode = new XElement(_tableIndexXNS + "f",
                        new XElement(_tableIndexXNS + "foN", folder),
                        new XElement(_tableIndexXNS + "fiN", fileName),
                        new XElement(_tableIndexXNS + "md5", CalculateHash(filePath)));
                    _fileIndexXDocument.Element(_tableIndexXNS + "fileIndex").Add(fileNode);
                });
                _fileIndexXDocument.Save(path);
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("IndexFiles Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("IndexFiles Failed: {0}", ex.Message) });
            }
            return result;
        }

        /// <summary>
        /// Calculates the hash
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string CalculateHash(string filePath)
        {
            string result = null;
            using (var stream = File.OpenRead(filePath))
            {
                MD5 md5 = MD5.Create();
                byte[] checsum = md5.ComputeHash(stream);
                result = BitConverter.ToString(checsum).Replace("-", string.Empty).ToUpper();
            }
            return result;
        }

        private string GetLogTemplate()
        {
            string result = null;
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(ResourceLogFile))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }
    }
}
