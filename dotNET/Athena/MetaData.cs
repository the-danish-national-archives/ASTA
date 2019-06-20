using Rigsarkiv.Athena.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        const string ResearchIndexColumnNode = "<column><columnID></columnID><missingValues></missingValues></column>";
        const string ResearchIndexValueNode = "<value></value>";
        const string CodeTableRow = "<row><c1></c1><c2></c2></row>";              
        const string Table = "table.xml";
        const string TableFolderPrefix = "table{0}";
        const string ColumnIDPrefix = "c{0}";
        const string PrimaryKeyPrefix = "PK_{0}";
        const string ForeignKeyPrefix = "FK_{0}_{1}_{2}";
        const string ReferencedTable = "{0}_{1}";
        const string ReferencedTableDescription = "Kodeliste til tabel {0}";
        const string VarCharPrefix = "VARCHAR({0})";
        private dynamic _metadata = null;        
        private string _tableXmlTemplate = null;
        private int _tablesCounter = 0;
        private XNamespace _tableIndexXNS = TableIndexXmlNs;

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
            var tableDocument = new XmlDocument();
            using (Stream stream = _assembly.GetManifestResourceStream(string.Format(ResourcePrefix, Table)))
            {
                tableDocument.Load(stream);
                _tableXmlTemplate = tableDocument.OuterXml;
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
            var message = result ? "End Converting Metadata" : "End Converting Metadata with errors";
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
            var researchIndexNode = new XElement(_tableIndexXNS + "table",
                new XElement(_tableIndexXNS + "tableID", folder),
                new XElement(_tableIndexXNS + "source", tableInfo["system"].ToString()),
                new XElement(_tableIndexXNS + "specialNumeric"));
            _researchIndexXDocument.Element(_tableIndexXNS + "researchIndex").Element(_tableIndexXNS + "mainTables").Add(researchIndexNode);            
            var tableNode = new XElement(_tableIndexXNS + "table",
                new XElement(_tableIndexXNS + "name", tableName),
                new XElement(_tableIndexXNS + "folder", folder),
                new XElement(_tableIndexXNS + "description", tableInfo["description"].ToString()),
                new XElement(_tableIndexXNS + "columns"),
                new XElement(_tableIndexXNS + "primaryKey", new XElement(_tableIndexXNS + "name", string.Format(PrimaryKeyPrefix, tableName))),
                new XElement(_tableIndexXNS + "foreignKeys"),
                new XElement(_tableIndexXNS + "rows", tableInfo["rows"].ToString()));
            _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Add(tableNode);
            foreach (var variable in (object[])tableInfo["variables"])
            {
                var variableInfo = (Dictionary<string, object>)variable;
                AddColumnNode(variableInfo,tableNode, researchIndexNode, tableName, index++);
            }
        }
        
        private void AddColumnNode(Dictionary<string, object> variableInfo, XElement tableNode, XElement researchIndexNode, string tableName, int index)
        {
            var columnName = variableInfo["name"].ToString();
            var columnType = GetMappedType(variableInfo);
            var columnNode = new XElement(_tableIndexXNS + "column",
                 new XElement(_tableIndexXNS + "name", columnName),
                 new XElement(_tableIndexXNS + "columnID", string.Format(ColumnIDPrefix, index)),
                 new XElement(_tableIndexXNS + "type", columnType),
                 new XElement(_tableIndexXNS + "typeOriginal", variableInfo["format"].ToString()),
                 new XElement(_tableIndexXNS + "nullable", variableInfo["nullable"].ToString().ToLower()),
                 new XElement(_tableIndexXNS + "description", variableInfo["description"].ToString()));
            tableNode.Element(_tableIndexXNS + "columns").Add(columnNode);
           
            researchIndexNode.Element(_tableIndexXNS + "specialNumeric").Value = variableInfo["specialNumeric"].ToString();
            if (!string.IsNullOrEmpty(variableInfo["refData"].ToString()) && !string.IsNullOrEmpty(variableInfo["refVariable"].ToString()))
            {
                var foreignKeyName = string.Format(ForeignKeyPrefix, tableName, columnName, index);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add foreign key: {0} ", foreignKeyName) });

                tableNode.Element(_tableIndexXNS + "foreignKeys").Add(new XElement(_tableIndexXNS + "foreignKey",
                    new XElement(_tableIndexXNS + "name", foreignKeyName),
                    new XElement(_tableIndexXNS + "referencedTable", variableInfo["refData"].ToString()),
                    new XElement(_tableIndexXNS + "reference", new XElement(_tableIndexXNS + "column", columnName), new XElement(_tableIndexXNS + "referenced", variableInfo["refVariable"].ToString()))));
            }
            AddKeys(variableInfo, tableNode, researchIndexNode, tableName, columnName, columnType, index);
        }       
        private void AddKeys(Dictionary<string, object> variableInfo, XElement tableNode, XElement researchIndexNode, string tableName ,string columnName, string columnType, int index)
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

                tableNode.Element(_tableIndexXNS + "foreignKeys").Add(new XElement(_tableIndexXNS + "foreignKey",
                    new XElement(_tableIndexXNS + "name", foreignKeyName),
                    new XElement(_tableIndexXNS + "referencedTable", refTableName),
                    new XElement(_tableIndexXNS + "reference", new XElement(_tableIndexXNS + "column", variableInfo["name"].ToString()), new XElement(_tableIndexXNS + "referenced", "Kode"))));
                AddReferencedTable(variableInfo, tableNode, researchIndexNode, tableName, columnName,columnType, refTableName, index);
            }
        }
        
        private void AddReferencedTable(Dictionary<string, object> variableInfo, XElement tableNode, XElement researchIndexNode, string tableName, string columnName,string columnType, string refTableName, int index)
        {
            _tablesCounter++;
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add referenced Table: {0} ", refTableName) });

            var folder = string.Format(TableFolderPrefix, _tablesCounter);
            Directory.CreateDirectory(string.Format(TablePath, _destFolderPath, folder));

            var options = (object[])variableInfo["options"];
            var optionsType = ParseOptions(options, researchIndexNode, folder, columnName);
            var columnNode1 = new XElement(_tableIndexXNS + "column", new XElement(_tableIndexXNS + "name", "Kode"),new XElement(_tableIndexXNS + "columnID", "c1"),new XElement(_tableIndexXNS + "type", columnType),new XElement(_tableIndexXNS + "typeOriginal"),new XElement(_tableIndexXNS + "nullable", "false"), new XElement(_tableIndexXNS + "description", "Kode"));
            var columnNode2 = new XElement(_tableIndexXNS + "column", new XElement(_tableIndexXNS + "name", "Kodeværdi"), new XElement(_tableIndexXNS + "columnID", "c2"), new XElement(_tableIndexXNS + "type", optionsType), new XElement(_tableIndexXNS + "typeOriginal"), new XElement(_tableIndexXNS + "nullable", "false"), new XElement(_tableIndexXNS + "description", "Kodeværdi"));
            var refTableNode = new XElement(_tableIndexXNS + "table",
                new XElement(_tableIndexXNS + "name", refTableName),
                new XElement(_tableIndexXNS + "folder", folder),
                new XElement(_tableIndexXNS + "description", string.Format(ReferencedTableDescription, tableName)),
                new XElement(_tableIndexXNS + "columns", columnNode1, columnNode2),
                new XElement(_tableIndexXNS + "primaryKey", new XElement(_tableIndexXNS + "name", string.Format(PrimaryKeyPrefix, refTableName)), new XElement(_tableIndexXNS + "column", "Kode")),
                new XElement(_tableIndexXNS + "rows", options.Length.ToString()));
            tableNode.Parent.Add(refTableNode);            
        }
        
        private string ParseOptions(object[] options, XElement researchIndexNode, string folder, string columnName)
        {
            var result = 0;
            var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", folder));
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add file: {0} ", path) });
            var tableDocument = new XmlDocument();
            tableDocument.LoadXml(_tableXmlTemplate);
            foreach (var option in options)
            {
                var code = (Dictionary<string, object>)option;
                var length = code["description"].ToString().Length;
                if(length > result) { result = length; }
                if((bool)code["isMissing"]) { AddMissingColumnNode(code["name"].ToString(), researchIndexNode, columnName); }
                var node = CreateNode(tableDocument, tableDocument.SelectSingleNode("//table"), CodeTableRow);
                node.SelectSingleNode("c1").InnerText = code["name"].ToString();
                node.SelectSingleNode("c2").InnerText = code["description"].ToString();
            }           
            tableDocument.DocumentElement.SetAttribute("xmlns", string.Format(TableXmlNs, folder));
            tableDocument.DocumentElement.SetAttribute("xsi:schemaLocation", string.Format(TableSchemaLocation, folder));
            tableDocument.Save(path);

            return string.Format(VarCharPrefix, result);
        }

        private void AddMissingColumnNode(string codeName, XElement researchIndexNode,string columnName)
        {

            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add missing value: {0} ", codeName) });
            XElement columnsNode = researchIndexNode.Element(_tableIndexXNS + "columns");
            XElement columnNode = new XElement(_tableIndexXNS + "column", new XElement(_tableIndexXNS + "columnID", columnName), new XElement(_tableIndexXNS + "missingValues"));
            if (columnsNode == null)
            {
                columnsNode = new XElement(_tableIndexXNS + "columns", columnNode);
                researchIndexNode.Add(columnsNode);
            }
            if (!columnsNode.Elements().Where(e => e.Element(_tableIndexXNS + "columnID").Value == columnName).Any())
            {
                columnsNode.Add(columnNode);
            }
            else
            {
                columnNode = columnsNode.Elements().Where(e => e.Element(_tableIndexXNS + "columnID").Value == columnName).FirstOrDefault();
            }
            var missingValueNode = new XElement(_tableIndexXNS + "value", codeName);
            columnNode.Element(_tableIndexXNS + "missingValues").Add(missingValueNode);
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
                        var regExSplit = ((object[])variableInfo["regExps"])[index].ToString().Split(',');
                        var length = regExSplit[1].Split('}');
                        result = string.Format(VarCharPrefix,length);
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
