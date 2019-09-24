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
        const string ReservedWordPattern = "^(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)$";
        protected Regex _reservedWord = null;
        private StringBuilder _variables = null;
        private StringBuilder _descriptions = null;
        private StringBuilder _codeList = null;
        private StringBuilder _usercodes = null;

        public MetaData(LogManager logManager, string srcPath, string destPath, string destFolder, Report report) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Metadata";
            _report = report;
            _reservedWord = new Regex(ReservedWordPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
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
                _usercodes.AppendLine(string.Format("{0} {{{1}}}", column.Name, index));
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
            var path = string.Format(filePath, _destFolderPath, _report.ScriptType.ToString().ToLower(), NormalizeName(table.Name));
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
                if (_researchIndexXDocument == null) { throw new Exception("ResearchIndexXDocument property not setet"); }
                var path = string.Format(DataPath, _destFolderPath);
                _report.Tables.ForEach(table =>
                {
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Build {0} metadata", table.Folder) });
                    var tableNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "folder").Value == table.SrcFolder).FirstOrDefault();
                    var researchNode = _researchIndexXDocument.Element(_tableIndexXNS + "researchIndex").Element(_tableIndexXNS + "mainTables").Elements().Where(e => e.Element(_tableIndexXNS + "tableID").Value == table.SrcFolder).FirstOrDefault();
                    foreach (var columnNode in tableNode.Element(_tableIndexXNS + "columns").Elements())
                    {
                        var column = new Column() { Id = columnNode.Element(_tableIndexXNS + "columnID").Value, Name = columnNode.Element(_tableIndexXNS + "name").Value, Description = columnNode.Element(_tableIndexXNS + "description").Value, Type = columnNode.Element(_tableIndexXNS + "typeOriginal").Value, TypeOriginal = columnNode.Element(_tableIndexXNS + "type").Value };
                        EnsureType(column);
                        if (tableNode.Element(_tableIndexXNS + "foreignKeys").Elements().Any(e => e.Element(_tableIndexXNS + "reference").Element(_tableIndexXNS + "column").Value == column.Name))
                        {
                            var foreignKeyNode = tableNode.Element(_tableIndexXNS + "foreignKeys").Elements().Where(e => e.Element(_tableIndexXNS + "reference").Element(_tableIndexXNS + "column").Value == column.Name).FirstOrDefault();
                            column.CodeList = GetCodeList(foreignKeyNode, table, column);
                        }
                        column.MissingValues = GetMissingValues(researchNode, table, column);
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
            if(column.TypeOriginal.StartsWith(VarCharPrefix))
            {
                var regex = GetRegex(DataTypeIntPattern);
                if (regex.IsMatch(column.Type))
                {
                    column.TypeOriginal = "INTEGER";
                    column.Modified = true;
                }
                regex = GetRegex(DataTypeDecimalPattern);
                if (regex.IsMatch(column.Type))
                {
                    column.TypeOriginal = "DECIMAL";
                    column.Modified = true;
                }
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
            codelistName = codelistName.Substring(3 + tableName.Length + 1);
            codelistName = codelistName.Substring(0, codelistName.LastIndexOf("_"));
            if (_reservedWord.IsMatch(codelistName))
            {
                codelistName = string.Format("\"{0}\"", codelistName);
            }
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
