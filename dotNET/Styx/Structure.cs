using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private int _tablesCounter = 0;

        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public Structure(LogManager logManager, string srcPath, string destPath, string destFolder, ScriptType scriptType) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Structure";
            _report.ScriptType = scriptType;
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
            if (EnsureRootFolder() && EnsureTables() && EnsureScripts())
            {
                result = true;
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

        private bool EnsureTables()
        {
            var result = true;
            try
            {
                var path = string.Format(DataPath, _destFolderPath);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Ensure Tables: {0}", path) });
                Directory.CreateDirectory(path);
                _researchIndexXDocument = XDocument.Load(string.Format(ResearchIndexPath, _srcPath));
                _tableIndexXDocument = XDocument.Load(string.Format(TableIndexPath, _srcPath));
                foreach (var tableNode in _researchIndexXDocument.Element(_tableIndexXNS + "researchIndex").Element(_tableIndexXNS + "mainTables").Elements())
                {
                    _tablesCounter++;
                    var srcFolder = tableNode.Element(_tableIndexXNS + "tableID").Value;
                    var tableIndexNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "folder").Value == srcFolder).FirstOrDefault();
                    var tableName = tableIndexNode.Element(_tableIndexXNS + "name").Value;
                    var tableRows = int.Parse(tableIndexNode.Element(_tableIndexXNS + "rows").Value);
                    var folder = string.Format(TableFolderPrefix, _report.ScriptType.ToString().ToLower(), tableName);
                    var folderPath = string.Format("{0}\\{1}", path,folder);
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Ensure Table: {0}", folderPath) });
                    Directory.CreateDirectory(folderPath);                                        
                    _report.Tables.Add(new Table() { Folder = folder, SrcFolder = srcFolder, Name = tableName, Rows = tableRows, Columns = new List<Column>() });                    
                }
            }
            catch (IOException ex)
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
                if(_report.ScriptType == ScriptType.SPSS)
                {
                    var path = string.Format(DataPath, _destFolderPath);
                    var content = GetScriptTemplate();
                    _report.Tables.ForEach(table =>
                    {
                        var folderPath = string.Format("{0}\\{1}", path, table.Folder);
                        var filePath = string.Format("{0}\\{1}.{2}", folderPath, table.Folder, ext);
                        using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
                        {
                            sw.Write(string.Format(content, "", folderPath, table.Folder));
                        }
                    });
                }
            }
            catch (IOException ex)
            {
                result = false;
                _log.Error("EnsureScripts Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureScripts Failed: {0}", ex.Message) });
            }
            return result;
        }
    }
}
