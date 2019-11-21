using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Styx.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Rigsarkiv.Styx
{
    /// <summary>
    /// MetaData Converter
    /// </summary>
    public class MetaData : Converter
    {
        const string VariablesPath = "{0}\\Data\\{1}_{2}\\{1}_{2}_VARIABEL.txt";
        const string DescriptionsPath = "{0}\\Data\\{1}_{2}\\{1}_{2}_VARIABELBESKRIVELSE.txt";
        const string OldTypeStringPattern = "^(CHAR|CHARACTER|CHAR VARYING|CHARACTER VARYING|VARCHAR|NATIONAL CHARACTER|NATIONAL CHAR|NCHAR|NATIONAL CHARACTER VARYING|NATIONAL CHAR VARYING|NCHAR VARYING)$";
        const string OldTypeIntPattern = "^(INTEGER|INT|SMALLINT)$";
        const string OldTypeDecimalPattern = "^(NUMERIC|DECIMAL|DEC|FLOAT|REAL|DOUBLE PRECISION)$";
        const string OldTypeBooleanPattern = "^(BOOLEAN)$";
        const string OldTypeDatePattern = "^(DATE)$";
        const string OldTypeTimePattern = "^(TIME|TIME\\[WITH TIME ZONE\\])$";
        const string OldTypeDateTimePattern = "^(TIMESTAMP|TIMESTAMP\\[WITH TIME ZONE\\])$";
        private StringBuilder _variables = null;
        private StringBuilder _descriptions = null;
        private StringBuilder _codeList = null;
        private StringBuilder _usercodes = null;

        public MetaData(LogManager logManager, string srcPath, string destPath, string destFolder, Report report, FlowState state) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Metadata";
            _report = report;
            _state = state;
            _variables = new StringBuilder();
            _descriptions = new StringBuilder();
            _codeList = new StringBuilder();
            _usercodes = new StringBuilder();            
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
            result = (_state == FlowState.Running || _state == FlowState.Suspended) ? EnsureTables() : true;
            if ((_state == FlowState.Running || _state == FlowState.Completed) && result)
            {
                result = EnsureFiles();
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
                    _usercodes.Clear();
                    EnsureFiles(table);
                    if(_report.ScriptType == ScriptType.SPSS) { EnsureUserCodesFile(table); }
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

        private void EnsureUserCodesFile(Table table)
        {
            var index = 0;
            table.Columns.Where(c => c.MissingValues != null).ToList().ForEach(column =>
            {
                _usercodes.AppendLine(string.Format("{0} {{{1}}}", NormalizeName(column.Name), index));
                index++;
            });
            if (_usercodes.Length > 0)
            {
                EnsureFile(table, UserCodesPath, _usercodes.ToString());
            }
        }

        private void EnsureFiles(Table table)
        {
            var index = 0;
            table.Columns.ForEach(column =>
            {                
                var codeList = string.Empty;
                if (column.CodeList != null)
                {
                    var codelistName = NormalizeName(column.CodeList.Name);
                    if (_report.ScriptType == ScriptType.SPSS) { codelistName = NormalizeName(column.Name); }
                    codeList = string.Format("{0}{1}.", column.TypeOriginal.StartsWith("VARCHAR") ? "$" : string.Empty, codelistName);
                    _codeList.AppendLine(codelistName);
                    _codeList.AppendLine(string.Format("{{{0}}}", index));
                    index++;
                }
                _variables.AppendLine(string.Format("{0} {1} {2}", NormalizeName(column.Name), GetColumnType(column), codeList));
                _descriptions.AppendLine(string.Format("{0} '{1}'", NormalizeName(column.Name), column.Description));
            });
            EnsureFile(table, VariablesPath, _variables.ToString());
            EnsureFile(table, DescriptionsPath, _descriptions.ToString());
            EnsureFile(table, CodeListPath, _codeList.ToString());
        }

        private void EnsureFile(Table table,string filePath,string content)
        {
            var path = string.Format(filePath, _destFolderPath, _report.ScriptType.ToString().ToLower(), _state == FlowState.Completed ? NormalizeName(table.Title) : NormalizeName(table.Name));
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add file: {0}", path) });
            using (var sw = new StreamWriter(path, true, Encoding.UTF8))
            {
                sw.Write(content);
            }
        }

        private bool EnsureTables()
        {
            var result = true;
            try
            {
                if (_state == FlowState.Running && _researchIndexXDocument == null) { throw new Exception("ResearchIndexXDocument property not setet"); }
                var path = string.Format(DataPath, _destFolderPath);
                _report.Tables.ForEach(table =>
                {
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Build {0} metadata", table.Folder) });
                    var tableNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "folder").Value == table.SrcFolder).FirstOrDefault();
                    XElement researchNode = _state == FlowState.Running ? _researchIndexXDocument.Element(_tableIndexXNS + "researchIndex").Element(_tableIndexXNS + "mainTables").Elements().Where(e => e.Element(_tableIndexXNS + "tableID").Value == table.SrcFolder).FirstOrDefault() : null;
                    foreach (var columnNode in tableNode.Element(_tableIndexXNS + "columns").Elements())
                    {
                        var column = new Column() { Id = columnNode.Element(_tableIndexXNS + "columnID").Value, Name = columnNode.Element(_tableIndexXNS + "name").Value, Description = columnNode.Element(_tableIndexXNS + "description").Value, Type = columnNode.Element(_tableIndexXNS + "typeOriginal").Value, TypeOriginal = columnNode.Element(_tableIndexXNS + "type").Value };
                        column.IsKey = tableNode.Element(_tableIndexXNS + "primaryKey").Element(_tableIndexXNS + "column").Value == column.Name;
                        EnsureType(column);
                        if (tableNode.Element(_tableIndexXNS + "foreignKeys").Elements().Any(e => e.Element(_tableIndexXNS + "reference").Element(_tableIndexXNS + "column").Value == column.Name))
                        {
                            var foreignKeyNode = tableNode.Element(_tableIndexXNS + "foreignKeys").Elements().Where(e => e.Element(_tableIndexXNS + "reference").Element(_tableIndexXNS + "column").Value == column.Name).FirstOrDefault();
                            column.CodeList = GetCodeList(foreignKeyNode, table, column);
                        }
                        if (_state == FlowState.Running) { column.MissingValues = GetMissingValues(researchNode, table, column); }
                        table.Columns.Add(column);
                    }
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

        private void EnsureType(Column column)
        {
            if(_state == FlowState.Suspended) { UpdateOldType(column); }
            if(column.TypeOriginal.StartsWith(VarCharPrefix))
            {
                var regex = GetRegex(DataTypeIntPattern);
                if (regex.IsMatch(column.Type))
                {
                    column.TypeOriginal = "INTEGER";
                    column.Modified = true;
                    return;
                }
                regex = GetRegex(DataTypeDecimalPattern);
                if (regex.IsMatch(column.Type))
                {
                    column.TypeOriginal = "DECIMAL";
                    column.Modified = true;
                    return;
                }
            }
        }

        private void UpdateOldType(Column column)
        {
            var result = string.Empty;
            var regex = GetRegex(OldTypeStringPattern);
            if (regex.IsMatch(column.TypeOriginal)) { result = string.Format("VARCHAR({0})", StringMaxLength); }
            regex = GetRegex(OldTypeIntPattern);
            if (regex.IsMatch(column.TypeOriginal)) { result = "INTEGER"; }
            regex = GetRegex(OldTypeDecimalPattern);
            if (regex.IsMatch(column.TypeOriginal)) { result = "DECIMAL"; }
            regex = GetRegex(OldTypeBooleanPattern);
            if (regex.IsMatch(column.TypeOriginal)) { result = "VARCHAR(5)"; }
            regex = GetRegex(OldTypeDatePattern);
            if (regex.IsMatch(column.TypeOriginal)) { result = "DATE"; }
            regex = GetRegex(OldTypeTimePattern);
            if (regex.IsMatch(column.TypeOriginal)) { result = "TIME"; }
            regex = GetRegex(OldTypeDateTimePattern);
            if (regex.IsMatch(column.TypeOriginal)) { result = "TIMESTAMP"; }
            if (!string.IsNullOrEmpty(result) && column.TypeOriginal != result)
            {
                column.TypeOriginal = result;
                column.Modified = true;
            }
        }

        private Dictionary<string, string> GetMissingValues(XElement tableNode, Table table, Column column)
        {
            if (tableNode.Element(_tableIndexXNS + "columns") == null) { return null; }
            if (!tableNode.Element(_tableIndexXNS + "columns").Elements().Any(e => e.Element(_tableIndexXNS + "columnID").Value == column.Id))
            {
                return null;
            }
            var result = new Dictionary<string, string>();
            var columnNode = tableNode.Element(_tableIndexXNS + "columns").Elements().Where(e => e.Element(_tableIndexXNS + "columnID").Value == column.Id).FirstOrDefault();
            foreach (var valueNode in columnNode.Element(_tableIndexXNS + "missingValues").Elements())
            {
                result.Add(valueNode.Value, valueNode.Value);
            }
            return result;
        }

        private Table GetCodeList(XElement foreignKeyNode, Table table, Column column)
        {
            var referencedTable = foreignKeyNode.Element(_tableIndexXNS + "referencedTable").Value;
            if(_report.Tables.Any(t => t.Name == referencedTable))
            {
                return null;
            }
            var result = new Table() { Columns = new List<Column>(), RowsCounter = 0 };
            var tableName = NormalizeName(table.Name);
            var codelistName = foreignKeyNode.Element(_tableIndexXNS + "name").Value;
            result.Title = codelistName;
            result.Title = result.Title.Substring(3);
            result.Title = result.Title.Substring(0, result.Title.LastIndexOf("_"));
            codelistName = codelistName.Substring(3 + tableName.Length + 1);
            codelistName = codelistName.Substring(0, codelistName.LastIndexOf("_"));
            result.Name = codelistName;

            var tableNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "name").Value == referencedTable).FirstOrDefault();
            result.SrcFolder = tableNode.Element(_tableIndexXNS + "folder").Value;
            result.Rows = int.Parse(tableNode.Element(_tableIndexXNS + "rows").Value);
            foreach (var columnNode in tableNode.Element(_tableIndexXNS + "columns").Elements())
            {
                var codeListColumn = new Column() { Id = columnNode.Element(_tableIndexXNS + "columnID").Value, Name = columnNode.Element(_tableIndexXNS + "name").Value, Description = columnNode.Element(_tableIndexXNS + "description").Value, Type = columnNode.Element(_tableIndexXNS + "typeOriginal").Value, TypeOriginal = columnNode.Element(_tableIndexXNS + "type").Value };
                codeListColumn.IsKey = tableNode.Element(_tableIndexXNS + "primaryKey").Element(_tableIndexXNS + "column").Value == codeListColumn.Name;
                result.Columns.Add(codeListColumn);
            }
            return result;
        }
    }
}
