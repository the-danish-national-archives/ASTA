using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Styx.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rigsarkiv.Styx
{
    /// <summary>
    /// MetaData Converter
    /// </summary>
    public class MetaData : Converter
    {
        const string TableIndexPath = "{0}\\Indices\\tableIndex.xml";
        private XDocument _tableIndexXDocument = null;

        public MetaData(LogManager logManager, string srcPath, string destPath, string destFolder, Report report) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Metadata";
            _report = report;
        }

        /// <summary>
        /// start converter
        /// </summary>
        public override bool Run()
        {
            var result = false;
            var message = string.Format("Start Converting Metadata {0} -> {1}", _srcFolder, _destFolder);
            _log.Info(message);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
            if (EnsureTables())
            {
                result = true;
            }
            message = result ? "End Converting Metadata" : "End Converting Metadata with errors";
            _log.Info(message);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
            return result;
        }

        private bool EnsureTables()
        {
            var result = true;
            try
            {
                _tableIndexXDocument = XDocument.Load(string.Format(TableIndexPath, _srcPath));
                var path = string.Format(DataPath, _destFolderPath);
                _report.Tables.ForEach(table =>
                {
                    var folderPath = string.Format("{0}\\{1}", path, table.Folder);
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add metadata: {0}", folderPath) });
                    var tableNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "folder").Value == table.SrcFolder).FirstOrDefault();
                    table.Name = tableNode.Element(_tableIndexXNS + "name").Value;
                    EnsureTable(tableNode, table);
                });
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("EnsureTables Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureTables Failed: {0}", ex.Message) });
            }
            return result;
        }

        private void EnsureTable(XElement tableNode, Table table)
        {
            foreach (var columnNode in tableNode.Element(_tableIndexXNS + "columns").Elements())
            {
                var column = new Column();
                column.Id = columnNode.Element(_tableIndexXNS + "columnID").Value;
                column.Name = columnNode.Element(_tableIndexXNS + "name").Value;
                column.Type = columnNode.Element(_tableIndexXNS + "typeOriginal").Value;
                column.TypeOriginal = columnNode.Element(_tableIndexXNS + "type").Value;
                if (tableNode.Element(_tableIndexXNS + "foreignKeys").Elements().Any(e => e.Element(_tableIndexXNS + "reference").Element(_tableIndexXNS + "column").Value == column.Name))
                {
                    var foreignKeyNode = tableNode.Element(_tableIndexXNS + "foreignKeys").Elements().Where(e => e.Element(_tableIndexXNS + "reference").Element(_tableIndexXNS + "column").Value == column.Name).FirstOrDefault();
                    column.CodeList = GetCodeList(foreignKeyNode, table, column);
                }
                table.Columns.Add(column);
            }
        }

        private Table GetCodeList(XElement foreignKeyNode, Table table, Column column)
        {
            var result = new Table() { Columns = new List<Column>() };

            var codelistName = foreignKeyNode.Element(_tableIndexXNS + "name").Value;
            codelistName = codelistName.Substring(3 + table.Name.Length + 1);
            codelistName = codelistName.Substring(0, codelistName.LastIndexOf("_"));
            result.Name = codelistName;

            var referencedTable = foreignKeyNode.Element(_tableIndexXNS + "referencedTable").Value;
            var tableNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "name").Value == referencedTable).FirstOrDefault();
            result.SrcFolder = tableNode.Element(_tableIndexXNS + "folder").Value;

            return result;
        }
    }
}
