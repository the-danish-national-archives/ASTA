using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Asta.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// MetaData Converter
    /// </summary>
    public class MetaData : Converter
    {
        const string TableFolderPrefix = "table{0}";
        const string ColumnIDPrefix = "c{0}";
        const string PrimaryKeyPrefix = "PK_{0}";
        const string ForeignKeyPrefix = "FK_{0}_{1}_{2}";
        const string ReferencedTable = "{0}_{1}";
        const string ReferencedTableDescription = "Kodeliste til tabel {0}";
        const string Code = "Kode";
        const string CodeValue = "Kodeværdi";
        const string EnclosedReservedWordPattern = "^(\")(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)(\")$";
        private dynamic _metadata = null;        
        private int _tablesCounter = 0;
        private Regex _enclosedReservedWord = null;

        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public MetaData(LogManager logManager, string srcPath, string destPath, string destFolder) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Metadata";            
            using (Stream stream = _assembly.GetManifestResourceStream(string.Format(ResourcePrefix, TableIndex)))
            {
                _tableIndexXDocument = XDocument.Load(stream);
            }
            using (Stream stream = _assembly.GetManifestResourceStream(string.Format(ResourcePrefix, ResearchIndex)))
            {
                _researchIndexXDocument = XDocument.Load(stream);
            }
            _enclosedReservedWord = new Regex(EnclosedReservedWordPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// start converter
        /// </summary>
        /// <returns></returns>
        public override bool Run()
        {
            var result = false;
            var message = string.Format("Start Converting Metadata {0} -> {1}", _srcFolder, _destFolder);
            _log.Info(message);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
            if (LoadJson() && EnsureTables())
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
                var path = string.Format(IndicesPath, _destFolderPath);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Update tableIndex.xml: {0}", path) });
                foreach (var table in (object[])_metadata)
                {
                    var tableInfo = (Dictionary<string, object>)table;
                   AddTableNode(tableInfo);
                }
                 _tableIndexXDocument.Save(string.Format("{0}\\{1}", path, TableIndex));
                _researchIndexXDocument.Save(string.Format("{0}\\{1}", path, ResearchIndex));
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("EnsureTableIndex Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureTableIndex Failed: {0}", ex.Message) });
            }
            return result;
        }        

        private void AddTableNode(Dictionary<string, object> tableInfo)
        {
            var index = 1;
            _tablesCounter++;
            var folder = string.Format(TableFolderPrefix, _tablesCounter);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add {0} to tableIndex.xml", folder) });
            Directory.CreateDirectory(string.Format(TablePath, _destFolderPath, folder));
            var tableName = tableInfo["name"].ToString();
            var rows = tableInfo["rows"].ToString();
            var researchIndexNode = new XElement(_tableIndexXNS + "table",
                new XElement(_tableIndexXNS + "tableID", folder),
                new XElement(_tableIndexXNS + "source", tableInfo["system"].ToString()),
                new XElement(_tableIndexXNS + "specialNumeric", false.ToString().ToLower()));
            _researchIndexXDocument.Element(_tableIndexXNS + "researchIndex").Element(_tableIndexXNS + "mainTables").Add(researchIndexNode);
            var tableNode = new XElement(_tableIndexXNS + "table",
                new XElement(_tableIndexXNS + "name", tableName),
                new XElement(_tableIndexXNS + "folder", folder),
                new XElement(_tableIndexXNS + "description", tableInfo["description"].ToString()),
                new XElement(_tableIndexXNS + "columns"),
                new XElement(_tableIndexXNS + "primaryKey", new XElement(_tableIndexXNS + "name", string.Format(PrimaryKeyPrefix, NormalizeName(tableName)))),
                new XElement(_tableIndexXNS + "foreignKeys"),
                new XElement(_tableIndexXNS + "rows", rows));
            _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Add(tableNode);
            var srcFolder = tableInfo["fileName"].ToString();
            srcFolder = srcFolder.Substring(0, srcFolder.LastIndexOf("."));
            var table = new Table() { SrcFolder = srcFolder, Name = tableName, Folder = folder, Rows = int.Parse(rows), Errors = 0, RowsCounter = 0, Columns = new List<Column>(), ErrorsRows = new Dictionary<string, Row>() };
            _report.Tables.Add(table);
            foreach (var variable in (object[])tableInfo["variables"])
            {
                 AddColumnNode((Dictionary<string, object>)variable, tableNode, researchIndexNode, table, index++);
            }
            if (tableInfo["references"] != null && ((object[])tableInfo["references"]).Length > 0)
            {
                foreach (var reference in (object[])tableInfo["references"])
                {
                    AddReferenceNode((Dictionary<string, object>)reference, tableNode, table, index++);
                }
            }
        }

        private void AddReferenceNode(Dictionary<string, object> referenceInfo, XElement tableNode, Table table, int index)
        {
            var referencedTable = referenceInfo["refTable"].ToString();
            var foreignKeyName = string.Format(ForeignKeyPrefix, NormalizeName(table.Name), NormalizeName(referencedTable), index);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add foreign key: {0} ", foreignKeyName) });

            tableNode.Element(_tableIndexXNS + "foreignKeys").Add(new XElement(_tableIndexXNS + "foreignKey",
                   new XElement(_tableIndexXNS + "name", foreignKeyName),
                   new XElement(_tableIndexXNS + "referencedTable", referencedTable)));
            var foreignKeyNode = tableNode.Element(_tableIndexXNS + "foreignKeys").Elements().Where(e => e.Element(_tableIndexXNS + "name").Value == foreignKeyName).FirstOrDefault();
            var refVariables = (object[])referenceInfo["refVariables"];
            var refKeys = (object[])referenceInfo["refKeys"];
            for (var i = 0; i < refVariables.Length; i++)
            {
                foreignKeyNode.Add(new XElement(_tableIndexXNS + "reference", new XElement(_tableIndexXNS + "column", refKeys[i].ToString()), new XElement(_tableIndexXNS + "referenced", refVariables[i].ToString())));
            }
        }

        private void AddColumnNode(Dictionary<string, object> variableInfo, XElement tableNode, XElement researchIndexNode, Table table, int index)
        {
            var columnName = variableInfo["name"].ToString();
            var columnDescription = variableInfo["description"].ToString();
            var columnType = GetMappedType(variableInfo);
            var columnId = string.Format(ColumnIDPrefix, index);
            var columnTypeOriginal = variableInfo["format"].ToString();
            var appliedRegExp = GetRegExp(variableInfo);
            var column = new Column() { Name = columnName, Id = columnId, Description = columnDescription, Type = columnType, TypeOriginal = columnTypeOriginal, Nullable = (bool)variableInfo["nullable"], HasSpecialNumeric = false, RegExp = appliedRegExp, Differences = 0, Errors = 0, ErrorsRows = new List<int>() };
            var columnNode = new XElement(_tableIndexXNS + "column",
                 new XElement(_tableIndexXNS + "name", columnName),
                 new XElement(_tableIndexXNS + "columnID", columnId),
                 new XElement(_tableIndexXNS + "type", columnType),
                 new XElement(_tableIndexXNS + "typeOriginal", columnTypeOriginal),
                 new XElement(_tableIndexXNS + "nullable", variableInfo["nullable"].ToString().ToLower()),
                 new XElement(_tableIndexXNS + "description", columnDescription));
            tableNode.Element(_tableIndexXNS + "columns").Add(columnNode);
            table.Columns.Add(column);            
            AddKeys(variableInfo, tableNode, researchIndexNode, table, column, index);
        }
        
        private void AddKeys(Dictionary<string, object> variableInfo, XElement tableNode, XElement researchIndexNode, Table table , Column column, int index)
        {
            if ((bool)variableInfo["isKey"])
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add primary Key columnName: {0} ", column.Name) });
                tableNode.Element(_tableIndexXNS + "primaryKey").Add(new XElement(_tableIndexXNS + "column", column.Name));
            }
            if (variableInfo["codeListKey"] != null && !string.IsNullOrEmpty(variableInfo["codeListKey"].ToString()))
            {
                var codeListKey = variableInfo["codeListKey"].ToString();
                var refTableName = string.Format(ReferencedTable, NormalizeName(table.Name), NormalizeName(codeListKey));
                var foreignKeyName = string.Format(ForeignKeyPrefix, NormalizeName(table.Name), NormalizeName(codeListKey), index);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add foreign key: {0} ", foreignKeyName) });
                if(table.CodeList == null) { table.CodeList = new List<Table>(); }
                tableNode.Element(_tableIndexXNS + "foreignKeys").Add(new XElement(_tableIndexXNS + "foreignKey",
                    new XElement(_tableIndexXNS + "name", foreignKeyName),
                    new XElement(_tableIndexXNS + "referencedTable", refTableName),
                    new XElement(_tableIndexXNS + "reference", new XElement(_tableIndexXNS + "column", variableInfo["name"].ToString()), new XElement(_tableIndexXNS + "referenced", "Kode"))));
                if(!table.CodeList.Any(t => t.Name == refTableName))
                {
                    AddReferencedTable(variableInfo, tableNode, researchIndexNode, table, column, refTableName, index);
                }
            }
        }
        
        private void AddReferencedTable(Dictionary<string, object> variableInfo, XElement tableNode, XElement researchIndexNode, Table table, Column column, string refTableName, int index)
        {
            _tablesCounter++;
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add referenced Table: {0} ", refTableName) });

            var folder = string.Format(TableFolderPrefix, _tablesCounter);
            Directory.CreateDirectory(string.Format(TablePath, _destFolderPath, folder));

            var options = (object[])variableInfo["options"];
            var codeList = new Table() { Name = refTableName, Folder = folder, Rows = options.Length, Errors = 0, RowsCounter = 0, Options= new List<string[]>(), ErrorsRows = new Dictionary<string, Row>() };
            codeList.Columns = new List<Column>() { (new Column() { Name = Code, Id = C1, Description = "Kode", Type = column.Type, TypeOriginal = "", HasSpecialNumeric = false, Differences = 0, Errors = 0, ErrorsRows = new List<int>() }), (new Column() { Name = CodeValue, Id = C2, Description = "Kodeværdi", Type = "", TypeOriginal = "", HasSpecialNumeric = false, Differences = 0, Errors = 0, ErrorsRows = new List<int>() }) };
            var optionsType = ParseOptions(options, researchIndexNode, codeList, folder, column);
            var columnNode1 = new XElement(_tableIndexXNS + "column", new XElement(_tableIndexXNS + "name", Code),new XElement(_tableIndexXNS + "columnID", C1),new XElement(_tableIndexXNS + "type", column.Type),new XElement(_tableIndexXNS + "typeOriginal"),new XElement(_tableIndexXNS + "nullable", "false"), new XElement(_tableIndexXNS + "description", "Kode"));
            var columnNode2 = new XElement(_tableIndexXNS + "column", new XElement(_tableIndexXNS + "name", CodeValue), new XElement(_tableIndexXNS + "columnID", C2), new XElement(_tableIndexXNS + "type", optionsType), new XElement(_tableIndexXNS + "typeOriginal"), new XElement(_tableIndexXNS + "nullable", "false"), new XElement(_tableIndexXNS + "description", "Kodeværdi"));
            var refTableNode = new XElement(_tableIndexXNS + "table",
                new XElement(_tableIndexXNS + "name", refTableName),
                new XElement(_tableIndexXNS + "folder", folder),
                new XElement(_tableIndexXNS + "description", string.Format(ReferencedTableDescription, table.Name)),
                new XElement(_tableIndexXNS + "columns", columnNode1, columnNode2),
                new XElement(_tableIndexXNS + "primaryKey", new XElement(_tableIndexXNS + "name", string.Format(PrimaryKeyPrefix, refTableName)), new XElement(_tableIndexXNS + "column", "Kode")),
                new XElement(_tableIndexXNS + "rows", options.Length.ToString()));
            tableNode.Parent.Add(refTableNode);
            codeList.Columns[1].Type = optionsType;
            table.CodeList.Add(codeList);
            foreach(var pair in codeList.Options)
            {
                if (RequiredSpecialNumeric(column, pair[0])) { EnableSpecialNumeric(column, tableNode, researchIndexNode, pair[0]); }
                if (column.HasSpecialNumeric) { AddMissingColumnNode(pair[0], researchIndexNode, column.Id); }
            }
        }
        
        private string ParseOptions(object[] options, XElement researchIndexNode, Table codeList, string folder, Column column)
        {
            var result = 0;
            var codeListColumn = codeList.Columns[0];
            var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", folder));
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Read codeList {0} options", codeList.Name) });
            foreach (var option in options)
            {
                var code = (Dictionary<string, object>)option;
                var value = code["name"].ToString();
                var description = code["description"].ToString();
                codeList.Options.Add(new string[2] { value, description });
                if (description.Length > result) { result = description.Length; }
                if((bool)code["isMissing"]) { AddMissingColumnNode(value, researchIndexNode, column.Id); }                
            }         
            return string.Format(VarCharPrefix, result);
        }        

        private string GetMappedType(Dictionary<string, object> variableInfo)
        {
            var result = "";
            switch (variableInfo["type"].ToString())
            {
                case "Int": result = "INTEGER"; break;
                case "Decimal": result = "DECIMAL"; break;
                case "Date": result = "DATE"; break;
                case "Time": result = "TIME"; break;
                case "DateTime": result = "TIMESTAMP"; break;
                case "String":
                    {
                        var regExSplit = GetRegExp(variableInfo);
                        result = string.Format(VarCharPrefix, GetColumnLength(VarCharPrefix, regExSplit));
                    };
                    break;
            }
            return result;
        }

        private string GetRegExp(Dictionary<string, object> variableInfo)
        {
            var index = (int)variableInfo["appliedRegExp"];
            if (index == -1)
            {
                index = 0;
                _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = string.Format("Variable: {0} has empty/null column data", variableInfo["name"].ToString()) });
            }
            if(variableInfo["type"].ToString() == "Decimal" && index == 2)
            {
                index = 0;
            }
            return ((object[])variableInfo["regExps"])[index].ToString();
        }

        private XmlNode CreateNode(XmlDocument xmlDocument,XmlNode parentNode,string xml)
        {
            var fragment = xmlDocument.CreateDocumentFragment();
            fragment.InnerXml = xml;
            return parentNode.AppendChild(fragment);
        }

        private string NormalizeName(string tableName)
        {
            var result = tableName;
            if (_enclosedReservedWord.IsMatch(tableName))
            {
                var groups = _enclosedReservedWord.Match(tableName).Groups;
                result = groups[2].Value;
            }
            return result;
        }

        private bool LoadJson()
        {
            var result = true;
            try
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Load metadata from: {0}", _srcPath) });
                var json = File.ReadAllText(_srcPath);
                JavaScriptSerializer jss = new JavaScriptSerializer();
                _metadata = jss.Deserialize<object>(json);
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("LoadJson Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("LoadJson Failed: {0}", ex.Message) });
            }
            return result;
        }
    }
}
