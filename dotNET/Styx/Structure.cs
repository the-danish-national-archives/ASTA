using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Styx.Entities;

namespace Rigsarkiv.Styx
{
    /// <summary>
    /// Structure Converter
    /// </summary>
    public class Structure : Converter
    {
        const string TableIndexPath = "{0}\\Indices\\tableIndex.xml";        
        const string ResearchIndexPath = "{0}\\Indices\\researchIndex.xml";
        const string TableFolderPrefix = "{0}_{1}";
        const string ContextDocumentationIndexPath = "{0}\\Indices\\contextDocumentationIndex.xml";
        const string ContextDocumentationPath = "{0}\\ContextDocumentation";
        const string ContextDocumentationPattern = "^[1-9]{1}[0-9]{0,}.(tif|mpg|mp3|jpg|jp2)$";
        private XDocument _contextDocumentationIndexXDocument = null;
        private Regex _contextDocumentationRegex = null;        

        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public Structure(LogManager logManager, string srcPath, string destPath, string destFolder, Report report, FlowState state) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Structure";
            if (report != null) { _report = report; }
            _state = state;
            _contextDocumentationRegex = new Regex(ContextDocumentationPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);            
        }

        /// <summary>
        /// start converter
        /// </summary>
        public override bool Run()
        {
            var result = false;
            var message = string.Format("Start Converting structure {0} -> {1}", _srcFolder, _destFolder);
            _log.Info(message);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
            if (EnsureRootFolder())
            {
                if (_state == FlowState.Created) { _state = File.Exists(string.Format(ResearchIndexPath, _srcPath)) ? FlowState.Running : FlowState.Suspended; }
                if (_state == FlowState.Suspended) { _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = "No Research Index file found" }); }
                result = EnsureTables();
                if ((_state == FlowState.Running || _state == FlowState.Completed) && result)
                {
                    result = EnsureScripts() && CopyFiles();
                }
            }
            message = result ? "End Converting structure" : "End Converting structure with errors";
            _log.Info(message);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
            return result;
        }

        private bool EnsureRootFolder()
        {
            var result = true;
            try
            {
                if (Directory.Exists(_destFolderPath))
                {
                    _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = string.Format("Delete exists Path: {0}", _destFolderPath) });
                    Directory.Delete(_destFolderPath, true);
                }
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Create path: {0}", _destFolderPath) });
                Directory.CreateDirectory(_destFolderPath);
            }
            catch (IOException ex)
            {
                result = false;
                _log.Error("EnsureRootFolder Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureRootFolder Failed: {0}", ex.Message) });
            }
            return result;
        }

        private bool CopyFiles()
        {
            var result = true;
            var srcPath = string.Format(ContextDocumentationPath, _srcPath);
            var destPath = string.Format(ContextDocumentationPath, _destFolderPath);
            try
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("create folder: {0}", destPath) });
                Directory.CreateDirectory(destPath);
                _report.ContextDocuments.Clear();
                var files = Getfiles();
                _contextDocumentationIndexXDocument = XDocument.Load(string.Format(ContextDocumentationIndexPath, _srcPath));
                foreach (var documentNode in _contextDocumentationIndexXDocument.Element(_tableIndexXNS + "contextDocumentationIndex").Elements())
                {
                    var id = documentNode.Element(_tableIndexXNS + "documentID").Value;
                    var documentTitle = documentNode.Element(_tableIndexXNS + "documentTitle").Value;
                    var title = ReplaceInvalidChars(documentTitle);
                    if(files.ContainsKey(id) && files[id].Count > 0)
                    {
                        var docFolderPath = string.Format("{0}\\{1}", destPath, title);
                        _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("create folder: {0}", docFolderPath) });
                        Directory.CreateDirectory(docFolderPath);
                        _report.ContextDocuments.Add(string.Format("{0}", documentTitle), string.Format("{0}",title));
                        files[id].ForEach(srcFilePath => {
                            var fileExt = srcFilePath.Substring(srcFilePath.LastIndexOf(".") + 1);
                            var destFilePath = string.Format("{0}\\{1}", docFolderPath, srcFilePath.Substring(srcFilePath.LastIndexOf("\\") + 1));
                            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("copy file {0} -> {1}", srcFilePath, destFilePath) });
                            File.Copy(srcFilePath, destFilePath, true);
                        });                        
                    }                    
                }
                
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("CopyFiles Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("CopyFiles Failed: {0}", ex.Message) });
            }
            return result;
        }

        private string ReplaceInvalidChars(string fileName)
        {
            var result = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            if(result != fileName)
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = string.Format("file renamed {0} -> {1}", fileName, result) });
            }
            return result;
        }

        private Dictionary<string,List<string>> Getfiles()
        {
            var result = new Dictionary<string, List<string>>();
            var srcPath = string.Format(ContextDocumentationPath, _srcPath);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Loop through files at: {0}", srcPath) });
            foreach (string filePath in Directory.GetFiles(srcPath, "*.*", SearchOption.AllDirectories))
            {
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
                if(_contextDocumentationRegex.IsMatch(fileName))
                {
                    var id = filePath.Substring(0, filePath.Length - (fileName.Length + 1));
                    id = id.Substring(id.LastIndexOf("\\") + 1);
                    if(!result.ContainsKey(id))
                    {
                        result.Add(id, new List<string>());
                    }
                    result[id].Add(filePath);
                }
            }
            return result;
        }

        private List<XElement> GetTablesNodes()
        {
            var result = new List<XElement>();
            _tableIndexXDocument = XDocument.Load(string.Format(TableIndexPath, _srcPath));

            if (_state == FlowState.Running)
            {
                _researchIndexXDocument = XDocument.Load(string.Format(ResearchIndexPath, _srcPath));
                _researchIndexXDocument.Element(_tableIndexXNS + "researchIndex").Element(_tableIndexXNS + "mainTables").Elements().ToList().ForEach(tableNode => {
                    var srcFolder = tableNode.Element(_tableIndexXNS + "tableID").Value;
                    var tableIndexNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "folder").Value == srcFolder).FirstOrDefault();
                    result.Add(tableIndexNode);
                });
            }
            else
            {
               _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().ToList().ForEach(tableNode => {
                if(tableNode.Element(_tableIndexXNS + "foreignKeys") != null)
                {
                    result.Add(tableNode);
                }
               });
            }
            return result;
        }

        private bool EnsureTables()
        {
            var result = true;
            try
            {
                var path = string.Format(DataPath, _destFolderPath);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Ensure Tables: {0}", path) });
                Directory.CreateDirectory(path);
                if (_state == FlowState.Running || _state == FlowState.Suspended) {
                    foreach (var tableIndexNode in GetTablesNodes())
                    {
                        var srcFolder = tableIndexNode.Element(_tableIndexXNS + "folder").Value;
                        var tableName = tableIndexNode.Element(_tableIndexXNS + "name").Value;
                        var tableRows = int.Parse(tableIndexNode.Element(_tableIndexXNS + "rows").Value);
                        var folder = string.Format(TableFolderPrefix, _report.ScriptType.ToString().ToLower(), NormalizeName(tableName));
                        _report.Tables.Add(new Table() { Folder = folder, SrcFolder = srcFolder, Name = tableName, Rows = tableRows, RowsCounter = 0, Columns = new List<Column>() });
                    }
                }
                if (_state == FlowState.Running || _state == FlowState.Completed)
                {
                    _report.Tables.ForEach(table => {
                        if(string.IsNullOrEmpty(table.Folder)) { table.Folder = string.Format(TableFolderPrefix, _report.ScriptType.ToString().ToLower(), NormalizeName(table.Name)); }
                        var folderPath = string.Format("{0}\\{1}", path, table.Folder);
                        _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Ensure Table: {0}", folderPath) });
                        Directory.CreateDirectory(folderPath);
                    });
                }
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("EnsureTables Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureTables Failed: {0}", ex.Message) });
            }
            return result;
        }

        private bool EnsureScripts()
        {
            var result = true;
            string ext = null;
            switch (_report.ScriptType)
            {
                case ScriptType.SPSS: ext = "sps"; break;
                case ScriptType.SAS: ext = "sas"; ; break;
                case ScriptType.Stata: ext = "do"; ; break;
            }
            try
            {
                //TODO remove following if all scripts ready
                if(_report.ScriptType == ScriptType.SPSS)
                {
                    var path = string.Format(DataPath, _destFolderPath);
                    var content = GetScriptTemplate();
                    _report.Tables.ForEach(table =>
                    {
                        var folderPath = string.Format("{0}\\{1}", path, table.Folder);
                        var filePath = string.Format("{0}\\{1}.{2}", folderPath, table.Folder, ext);
                        _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Ensure Script file: {0}", filePath) });

                        using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
                        {
                            sw.Write(string.Format(content, "", folderPath, table.Folder));
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("EnsureScripts Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureScripts Failed: {0}", ex.Message) });
            }
            return result;
        }        
    }
}
