using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Styx.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Rigsarkiv.Styx
{
    /// <summary>
    /// MetaData Converter
    /// </summary>
    public class MetaData : Converter
    {
        const string TableIndexPath = "{0}\\Indices\\tableIndex.xml";
        const string VariablesPath = "{0}\\Data\\{1}\\{2}_VARIABEL.txt";
        const string DescriptionsPath = "{0}\\Data\\{1}\\{2}_VARIABELBESKRIVELSE.txt";        
        private XDocument _tableIndexXDocument = null;
        private StringBuilder _variables = null;
        private StringBuilder _descriptions = null;
        private StringBuilder _codeList = null;

        public MetaData(LogManager logManager, string srcPath, string destPath, string destFolder, Report report) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Metadata";
            _report = report;
            _variables = new StringBuilder();
            _descriptions = new StringBuilder();
            _codeList = new StringBuilder();
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
            if (EnsureTables() && EnsureFiles())
            {
                result = true;
            }
            message = result ? "End Converting Metadata" : "End Converting Metadata with errors";
            _log.Info(message);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
            return result;
        }

        private bool EnsureFiles()
        {
            var result = true;
            try
            {
                _report.Tables.ForEach(table =>
                {
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Write {0} files", table.Folder) });
                    _variables.Clear();
                    _descriptions.Clear();
                    _codeList.Clear();
                    EnsureFiles(table);
                });
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("EnsureFiles Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureFiles Failed: {0}", ex.Message) });
            }
            return result;
        }

        private void EnsureFiles(Table table)
        {
            var index = 0;
            table.Columns.ForEach(column =>
            {
                var codeList = string.Empty;
                if (column.CodeList != null)
                {
                    codeList = string.Format("{0}{1}.", column.TypeOriginal.StartsWith("VARCHAR") ? "$" : string.Empty, column.CodeList.Name);
                    _codeList.AppendLine(column.CodeList.Name);
                    _codeList.AppendLine(string.Format("{{{0}}}", index));
                    index++;
                }
                _variables.AppendLine(string.Format("{0} {1} {2}", column.Name, GetColumnType(column), codeList));
                _descriptions.AppendLine(string.Format("{0} '{1}'", column.Name, column.Description));
            });
            EnsureFile(table, VariablesPath, _variables.ToString());
            EnsureFile(table, DescriptionsPath, _descriptions.ToString());
            EnsureFile(table, CodeListPath, _codeList.ToString());
        }

        private void EnsureFile(Table table,string filePath,string content)
        {
            var path = string.Format(filePath, _destFolderPath, table.Folder, table.Name);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add file: {0}", path) });
            using (var sw = new StreamWriter(path, true))
            {
                sw.Write(content);
            }
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
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Build {0} metadata", table.Folder) });
                    var tableNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "folder").Value == table.SrcFolder).FirstOrDefault();
                    table.Name = tableNode.Element(_tableIndexXNS + "name").Value;
                    table.Rows = int.Parse(tableNode.Element(_tableIndexXNS + "rows").Value);
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
                column.Description = columnNode.Element(_tableIndexXNS + "description").Value;
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
            var referencedTable = foreignKeyNode.Element(_tableIndexXNS + "referencedTable").Value;
            if(_report.Tables.Any(t => t.Name == referencedTable))
            {
                return null;
            }
            var result = new Table() { Columns = new List<Column>() };

            var codelistName = foreignKeyNode.Element(_tableIndexXNS + "name").Value;
            codelistName = codelistName.Substring(3 + table.Name.Length + 1);
            codelistName = codelistName.Substring(0, codelistName.LastIndexOf("_"));
            result.Name = codelistName;

            var tableNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "name").Value == referencedTable).FirstOrDefault();
            result.SrcFolder = tableNode.Element(_tableIndexXNS + "folder").Value;
            result.Rows = int.Parse(tableNode.Element(_tableIndexXNS + "rows").Value);
            result.Columns.Add(new Column() { Name = column.Name, Id = C1, Type = column.Type });
            result.Columns.Add(new Column() { Name = column.Name, Id = C2, Type = column.Type });

            return result;
        }
    }
}
