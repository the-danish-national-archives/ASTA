using Rigsarkiv.Athena.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Xml;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// MetaData Converter
    /// </summary>
    public class MetaData : Converter
    {
        const string ResourcePrefix = "Rigsarkiv.Athena.Resources.{0}";
        const string ColumnNode = "<column><name></name><columnID></columnID><type></type><typeOriginal></typeOriginal><nullable></nullable><description></description></column>";
        const string TableNode = "<table><name></name><folder></folder><description></description><columns></columns><primaryKey><name></name></primaryKey><foreignKeys></foreignKeys><rows></rows></table>";
        const string PrimaryKeyColumnNode = "<column></column>";
        const string ForeignKeyNode = "<foreignKey><name></name><referencedTable></referencedTable><reference><column></column><referenced>Kode</referenced></reference></foreignKey>";
        const string ReferencedTableNode = "<table><name></name><folder></folder><description></description><columns><column><name>Kode</name><columnID>c1</columnID><type></type><typeOriginal></typeOriginal><nullable>false</nullable><description>Kode</description></column><column><name>Kodeværdi</name><columnID>c2</columnID><type></type><typeOriginal></typeOriginal><nullable>false</nullable><description>Kodeværdi</description></column></columns><primaryKey><name></name><column>Kode</column></primaryKey><rows></rows></table>";
        const string ResearchIndexTableNode = "<table><tableID></tableID><source></source></table>";
        const string ResearchIndexSspecialNumericNode = "<specialNumeric></specialNumeric>";
        const string ResearchIndexColumnsNode = "<columns></columns>";
        const string ResearchIndexColumnNode = "<column><columnID></columnID><missingValues></missingValues></column>";
        const string ResearchIndexValueNode = "<value></value>";
        const string CodeTableRow = "<row><c1></c1><c2></c2></row>";
        const string IndicesPath = "{0}\\Indices";
        const string TableIndex = "tableIndex.xml";
        const string ResearchIndex = "researchIndex.xml";
        const string TableIndexXmlNs = "http://www.sa.dk/xmlns/diark/1.0";
        const string Table = "table.xml";
        const string TableXmlNs = "http://www.sa.dk/xmlns/siard/1.0/schema0/{0}.xsd";
        const string TableSchemaLocation = "http://www.sa.dk/xmlns/siard/1.0/schema0/{0}.xsd {0}.xsd";
        const string TableFolderPrefix = "table{0}";
        const string ColumnIDPrefix = "c{0}";
        const string PrimaryKeyPrefix = "PK_{0}";
        const string ForeignKeyPrefix = "FK_{0}_{1}_{2}";
        const string ReferencedTable = "{0}_{1}";
        const string ReferencedTableDescription = "Kodeliste til tabel {0}";
        const string VarCharPrefix = "VARCHAR({0})";
        const string TablePath = "{0}\\Tables\\{1}";
        private dynamic _metadata = null;
        private XmlDocument _tableIndexDocument = null;
        private XmlDocument _researchIndexDocument = null;
        private string _tableXmlTemplate = null;
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
            var assembly = Assembly.GetExecutingAssembly();
            _logSection = "Metadata";
            _tableIndexDocument = new XmlDocument();
            using (Stream stream = assembly.GetManifestResourceStream(string.Format(ResourcePrefix, TableIndex)))
            {
                _tableIndexDocument.Load(stream);
            }
            _researchIndexDocument = new XmlDocument();
            using (Stream stream = assembly.GetManifestResourceStream(string.Format(ResourcePrefix, ResearchIndex)))
            {
                _researchIndexDocument.Load(stream);
            }
            var tableDocument = new XmlDocument();
            using (Stream stream = assembly.GetManifestResourceStream(string.Format(ResourcePrefix, Table)))
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
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Updade tableIndex.xml: {0}", path) });
                foreach (var table in (object[])_metadata)
                {
                    var tableInfo = (Dictionary<string, object>)table;
                    AddTableNode(tableInfo);
                }
                _researchIndexDocument.DocumentElement.SetAttribute("xmlns", TableIndexXmlNs);
                _tableIndexDocument.DocumentElement.SetAttribute("xmlns", TableIndexXmlNs);
                _tableIndexDocument.Save(string.Format("{0}\\{1}", path, TableIndex));
                _researchIndexDocument.Save(string.Format("{0}\\{1}", path, ResearchIndex));
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
            var researchIndexNode = CreateNode(_researchIndexDocument, _researchIndexDocument.SelectSingleNode("//mainTables"), ResearchIndexTableNode);
            researchIndexNode.SelectSingleNode("tableID").InnerText = folder;
            researchIndexNode.SelectSingleNode("source").InnerText = tableInfo["system"].ToString();
            var node = CreateNode(_tableIndexDocument, _tableIndexDocument.SelectSingleNode("//tables"), TableNode);
            node.SelectSingleNode("name").InnerText = tableName;
            node.SelectSingleNode("folder").InnerText = folder;
            node.SelectSingleNode("rows").InnerText = tableInfo["rows"].ToString();
            node.SelectSingleNode("description").InnerText = tableInfo["description"].ToString();
            node.SelectSingleNode("primaryKey/name").InnerText = string.Format(PrimaryKeyPrefix, tableName);
            foreach (var variable in (object[])tableInfo["variables"])
            {
                var variableInfo = (Dictionary<string, object>)variable;
                AddColumnNode(variableInfo,node,index++);
            }
        }

        private void AddColumnNode(Dictionary<string, object> variableInfo,XmlNode tableNode,int index)
        {
            var tableName = tableNode.SelectSingleNode("name").InnerText;
            var node = CreateNode(_tableIndexDocument, tableNode.SelectSingleNode("columns"), ColumnNode);
            var columnName = variableInfo["name"].ToString();
            var columnType = GetMappedType(variableInfo);
            node.SelectSingleNode("name").InnerText = columnName;
            node.SelectSingleNode("columnID").InnerText = string.Format(ColumnIDPrefix, index);
            node.SelectSingleNode("typeOriginal").InnerText = variableInfo["format"].ToString();
            node.SelectSingleNode("nullable").InnerText = variableInfo["nullable"].ToString().ToLower();
            node.SelectSingleNode("description").InnerText = variableInfo["description"].ToString();
            node.SelectSingleNode("type").InnerText = columnType;
            if (!string.IsNullOrEmpty(variableInfo["refData"].ToString()) && !string.IsNullOrEmpty(variableInfo["refVariable"].ToString()))
            {
                var foreignKeyName = string.Format(ForeignKeyPrefix, tableName, columnName, index);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add foreign key: {0} ", foreignKeyName) });
                var foreignKeyNode = CreateNode(_tableIndexDocument, tableNode.SelectSingleNode("foreignKeys"), ForeignKeyNode);
                foreignKeyNode.SelectSingleNode("name").InnerText = foreignKeyName;
                foreignKeyNode.SelectSingleNode("referencedTable").InnerText = variableInfo["refData"].ToString();
                foreignKeyNode.SelectSingleNode("reference/column").InnerText = columnName;
                foreignKeyNode.SelectSingleNode("reference/referenced").InnerText = variableInfo["refVariable"].ToString();
            }
            AddKeys(variableInfo, tableNode, tableName, columnName, columnType, index);
        }

        private void AddKeys(Dictionary<string, object> variableInfo, XmlNode tableNode,string tableName ,string columnName, string columnType, int index)
        {
            if ((bool)variableInfo["isKey"])
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add primary Key columnName: {0} ", columnName) });
                var primaryKeyNode = CreateNode(_tableIndexDocument, tableNode.SelectSingleNode("primaryKey"), PrimaryKeyColumnNode);
                primaryKeyNode.InnerText = columnName;
            }
            if (variableInfo["codeListKey"] != null && !string.IsNullOrEmpty(variableInfo["codeListKey"].ToString()))
            {
                var codeListKey = variableInfo["codeListKey"].ToString();
                var refTableName = string.Format(ReferencedTable, tableName, codeListKey);
                var foreignKeyName = string.Format(ForeignKeyPrefix, tableName, codeListKey, index);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add foreign key: {0} ", foreignKeyName) });

                var foreignKeyNode = CreateNode(_tableIndexDocument, tableNode.SelectSingleNode("foreignKeys"), ForeignKeyNode);
                foreignKeyNode.SelectSingleNode("name").InnerText = foreignKeyName;
                foreignKeyNode.SelectSingleNode("referencedTable").InnerText = refTableName;
                foreignKeyNode.SelectSingleNode("reference/column").InnerText = variableInfo["name"].ToString();

                AddReferencedTable(variableInfo, tableNode, tableName, columnType, refTableName, index);
            }
        }
        
        private void AddReferencedTable(Dictionary<string, object> variableInfo, XmlNode tableNode,string tableName, string columnType, string refTableName, int index)
        {
            _tablesCounter++;
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add referenced Table: {0} ", refTableName) });

            var folder = string.Format(TableFolderPrefix, _tablesCounter);
            Directory.CreateDirectory(string.Format(TablePath, _destFolderPath, folder));

            var refTableNode = CreateNode(_tableIndexDocument, tableNode.ParentNode, ReferencedTableNode);
            refTableNode.SelectSingleNode("name").InnerText = refTableName;
            refTableNode.SelectSingleNode("folder").InnerText = folder;
            refTableNode.SelectSingleNode("description").InnerText = string.Format(ReferencedTableDescription, tableName);
            refTableNode.SelectSingleNode("primaryKey/name").InnerText = string.Format(PrimaryKeyPrefix, refTableName);
            var columnNodes = refTableNode.SelectNodes("columns/column");
            columnNodes[0].SelectSingleNode("type").InnerText = columnType;
            columnNodes[1].SelectSingleNode("type").InnerText = ParseOptions((object[])variableInfo["options"], refTableNode, folder);
        }

        private string ParseOptions(object[] options, XmlNode refTableNode, string folder)
        {
            var result = 0;
            var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", folder));
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add file: {0} ", path) });
            var tableDocument = new XmlDocument();
            tableDocument.LoadXml(_tableXmlTemplate);
            refTableNode.SelectSingleNode("rows").InnerText = options.Length.ToString();
            foreach (var option in options)
            {
                var code = (Dictionary<string, object>)option;
                var length = code["description"].ToString().Length;
                if(length > result) { result = length; }

                var node = CreateNode(tableDocument, tableDocument.SelectSingleNode("//table"), CodeTableRow);
                node.SelectSingleNode("c1").InnerText = code["name"].ToString();
                node.SelectSingleNode("c2").InnerText = code["description"].ToString();
            }           
            tableDocument.DocumentElement.SetAttribute("xmlns", string.Format(TableXmlNs, folder));
            tableDocument.DocumentElement.SetAttribute("xsi:schemaLocation", string.Format(TableSchemaLocation, folder));
            tableDocument.Save(path);

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
