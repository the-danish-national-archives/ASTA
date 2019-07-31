using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Rigsarkiv.Asta.Logging;

namespace Rigsarkiv.Styx
{
    /// <summary>
    /// Structure Converter
    /// </summary>
    public class Structure : Converter
    {
        const string ResearchIndexPath = "{0}\\Indices\\researchIndex.xml";
        const string TableFolderPrefix = "table{0}";
        protected XDocument _researchIndexXDocument = null;
        private int _tablesCounter = 0;

        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public Structure(LogManager logManager, string srcPath, string destPath, string destFolder) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Structure";
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
            if (EnsureRootFolder() && EnsureTables())
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
                foreach(var table in _researchIndexXDocument.Element(_tableIndexXNS + "researchIndex").Element(_tableIndexXNS + "mainTables").Elements())
                {
                    _tablesCounter++;
                    var folder = string.Format(TableFolderPrefix, _tablesCounter);
                    var folderPath = string.Format("{0}\\{1}", path,folder);
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Ensure Table: {0}", folderPath) });
                    Directory.CreateDirectory(folderPath);
                    var srcFolder = table.Element(_tableIndexXNS + "tableID").Value;
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
    }
}
