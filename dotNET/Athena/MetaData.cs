using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        const string C1 = "c1";
        const string C2 = "c2";
        private dynamic _metadata = null;        
        private int _tablesCounter = 0;        

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
        }

        /// <summary>
        /// start converter
        /// </summary>
        /// <returns></returns>
        public override bool Run()
        {
            var result = false;
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Start Converting Metadata {0} -> {1}", _srcFolder, _destFolder) });
            if(LoadJson() && EnsureTables())
            {                
                result = true;
            }
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = (result ? "End Converting Metadata" : "End Converting Metadata with errors") });
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
                new XElement(_tableIndexXNS + "specialNumeric"),false.ToString().ToLower());
            _researchIndexXDocument.Element(_tableIndexXNS + "researchIndex").Element(_tableIndexXNS + "mainTables").Add(researchIndexNode);
            var tableNode = new XElement(_tableIndexXNS + "table",
                new XElement(_tableIndexXNS + "name", tableName),
                new XElement(_tableIndexXNS + "folder", folder),
                new XElement(_tableIndexXNS + "description", tableInfo["description"].ToString()),
                new XElement(_tableIndexXNS + "columns"),
                new XElement(_tableIndexXNS + "primaryKey", new XElement(_tableIndexXNS + "name", string.Format(PrimaryKeyPrefix, tableName))),
                new XElement(_tableIndexXNS + "foreignKeys"),
                new XElement(_tableIndexXNS + "rows", rows));
            _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Add(tableNode);
            var srcFolder = tableInfo["fileName"].ToString();
            srcFolder = srcFolder.Substring(0, srcFolder.LastIndexOf("."));
            var table = new Table() { SrcFolder = srcFolder, Name = tableName, Folder = folder, Rows = int.Parse(rows), Columns = new List<Column>() };
            _tables.Add(table);
            foreach (var variable in (object[])tableInfo["variables"])
            {
                var variableInfo = (Dictionary<string, object>)variable;
                AddColumnNode(variableInfo,tableNode, researchIndexNode, table, tableName, index++);
            }
        }
        
        private void AddColumnNode(Dictionary<string, object> variableInfo, XElement tableNode, XElement researchIndexNode, Table table,string tableName, int index)
        {
            var columnName = variableInfo["name"].ToString();
            var columnType = GetMappedType(variableInfo);
            var columnId = string.Format(ColumnIDPrefix, index);
            var columnTypeOriginal = variableInfo["format"].ToString();
            var appliedRegExp = ((object[])variableInfo["regExps"])[(int)variableInfo["appliedRegExp"]].ToString();
            var column = new Column() { Name = columnName, Id = columnId, Type = columnType, TypeOriginal = columnTypeOriginal, Nullable = (bool)variableInfo["nullable"], RegExp = appliedRegExp };
            var columnNode = new XElement(_tableIndexXNS + "column",
                 new XElement(_tableIndexXNS + "name", columnName),
                 new XElement(_tableIndexXNS + "columnID", columnId),
                 new XElement(_tableIndexXNS + "type", columnType),
                 new XElement(_tableIndexXNS + "typeOriginal", columnTypeOriginal),
                 new XElement(_tableIndexXNS + "nullable", variableInfo["nullable"].ToString().ToLower()),
                 new XElement(_tableIndexXNS + "description", variableInfo["description"].ToString()));
            tableNode.Element(_tableIndexXNS + "columns").Add(columnNode);
            table.Columns.Add(column);
            if (!string.IsNullOrEmpty(variableInfo["refData"].ToString()) && !string.IsNullOrEmpty(variableInfo["refVariable"].ToString()))
            {
                var foreignKeyName = string.Format(ForeignKeyPrefix, tableName, columnName, index);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add foreign key: {0} ", foreignKeyName) });

                tableNode.Element(_tableIndexXNS + "foreignKeys").Add(new XElement(_tableIndexXNS + "foreignKey",
                    new XElement(_tableIndexXNS + "name", foreignKeyName),
                    new XElement(_tableIndexXNS + "referencedTable", variableInfo["refData"].ToString()),
                    new XElement(_tableIndexXNS + "reference", new XElement(_tableIndexXNS + "column", columnName), new XElement(_tableIndexXNS + "referenced", variableInfo["refVariable"].ToString()))));
            }
            AddKeys(variableInfo, tableNode, researchIndexNode, table, tableName, columnId, columnName, columnType, index);
        }
        
        private void AddKeys(Dictionary<string, object> variableInfo, XElement tableNode, XElement researchIndexNode, Table table, string tableName ,string columnId, string columnName, string columnType, int index)
        {
            if ((bool)variableInfo["isKey"])
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add primary Key columnName: {0} ", columnName) });
                tableNode.Element(_tableIndexXNS + "primaryKey").Add(new XElement(_tableIndexXNS + "column", columnName));
            }
            if (variableInfo["codeListKey"] != null && !string.IsNullOrEmpty(variableInfo["codeListKey"].ToString()))
            {
                var codeListKey = variableInfo["codeListKey"].ToString();
                var refTableName = string.Format(ReferencedTable, tableName, codeListKey);
                var foreignKeyName = string.Format(ForeignKeyPrefix, tableName, codeListKey, index);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add foreign key: {0} ", foreignKeyName) });
                if(table.CodeList == null) { table.CodeList = new List<Table>(); }
                tableNode.Element(_tableIndexXNS + "foreignKeys").Add(new XElement(_tableIndexXNS + "foreignKey",
                    new XElement(_tableIndexXNS + "name", foreignKeyName),
                    new XElement(_tableIndexXNS + "referencedTable", refTableName),
                    new XElement(_tableIndexXNS + "reference", new XElement(_tableIndexXNS + "column", variableInfo["name"].ToString()), new XElement(_tableIndexXNS + "referenced", "Kode"))));
                AddReferencedTable(variableInfo, tableNode, researchIndexNode, table, tableName, columnId, columnName, columnType, refTableName, index);
            }
        }
        
        private void AddReferencedTable(Dictionary<string, object> variableInfo, XElement tableNode, XElement researchIndexNode, Table table, string tableName, string columnId, string columnName, string columnType, string refTableName, int index)
        {
            _tablesCounter++;
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add referenced Table: {0} ", refTableName) });

            var folder = string.Format(TableFolderPrefix, _tablesCounter);
            Directory.CreateDirectory(string.Format(TablePath, _destFolderPath, folder));

            var options = (object[])variableInfo["options"];
            var optionsType = ParseOptions(options, researchIndexNode, folder, columnId);
            var columnNode1 = new XElement(_tableIndexXNS + "column", new XElement(_tableIndexXNS + "name", Code),new XElement(_tableIndexXNS + "columnID", C1),new XElement(_tableIndexXNS + "type", columnType),new XElement(_tableIndexXNS + "typeOriginal"),new XElement(_tableIndexXNS + "nullable", "false"), new XElement(_tableIndexXNS + "description", "Kode"));
            var columnNode2 = new XElement(_tableIndexXNS + "column", new XElement(_tableIndexXNS + "name", CodeValue), new XElement(_tableIndexXNS + "columnID", C2), new XElement(_tableIndexXNS + "type", optionsType), new XElement(_tableIndexXNS + "typeOriginal"), new XElement(_tableIndexXNS + "nullable", "false"), new XElement(_tableIndexXNS + "description", "Kodeværdi"));
            var refTableNode = new XElement(_tableIndexXNS + "table",
                new XElement(_tableIndexXNS + "name", refTableName),
                new XElement(_tableIndexXNS + "folder", folder),
                new XElement(_tableIndexXNS + "description", string.Format(ReferencedTableDescription, tableName)),
                new XElement(_tableIndexXNS + "columns", columnNode1, columnNode2),
                new XElement(_tableIndexXNS + "primaryKey", new XElement(_tableIndexXNS + "name", string.Format(PrimaryKeyPrefix, refTableName)), new XElement(_tableIndexXNS + "column", "Kode")),
                new XElement(_tableIndexXNS + "rows", options.Length.ToString()));
            tableNode.Parent.Add(refTableNode);
            var columns = new List<Column>() { (new Column() { Name = Code, Id = C1, Type = columnType, TypeOriginal = "" }), (new Column() { Name = CodeValue, Id = C2, Type = optionsType, TypeOriginal = "" }) };
            table.CodeList.Add(new Table() { Name = refTableName, Folder = folder, Rows = options.Length, Columns = columns });
        }
        
        private string ParseOptions(object[] options, XElement researchIndexNode, string folder, string columnId)
        {
            var result = 0;
            var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", folder));
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add file: {0} ", path) });
            StartWriter(folder);            
            
            foreach (var option in options)
            {
                var code = (Dictionary<string, object>)option;
                var length = code["description"].ToString().Length;
                if(length > result) { result = length; }
                if((bool)code["isMissing"]) { AddMissingColumnNode(code["name"].ToString(), researchIndexNode, columnId); }
                _writer.WriteStartElement("row");
                _writer.WriteElementString("c1", code["name"].ToString());
                _writer.WriteElementString("c2", code["description"].ToString());
                _writer.WriteEndElement();
            }
            EndWriter();
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
                        var index = (int)variableInfo["appliedRegExp"];
                        var regExSplit = ((object[])variableInfo["regExps"])[index].ToString();
                        result = string.Format(VarCharPrefix, GetColumnLength(VarCharPrefix, regExSplit));
                    };
                    break;
            }
            return result;
        }

        private XmlNode CreateNode(XmlDocument xmlDocument,XmlNode parentNode,string xml)
        {
            var fragment = xmlDocument.CreateDocumentFragment();
            fragment.InnerXml = xml;
            return parentNode.AppendChild(fragment);
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
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("LoadJson Failed: {0}", ex.Message) });
            }
            return result;
        }
    }
}
