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
        const string IndicesPath = "{0}\\Indices";
        const string TableIndex = "tableIndex.xml";
        const string TableFolderPrefix = "table{0}";
        const string ColumnIDPrefix = "c{0}";
        const string PrimaryKeyPrefix = "PK_{0}";
        const string ForeignKeyPrefix = "PK_{0}_{1}_{2}";
        const string ReferencedTable = "{0}_{1}";
        const string ReferencedTableDescription = "Kodeliste til tabel {0}";
        const string VarCharPrefix = "VARCHAR({0})";
        private dynamic _metadata = null;
        private XmlDocument _tableDocument = null;
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
            _tableDocument = new XmlDocument();
            using (Stream stream = assembly.GetManifestResourceStream(string.Format(ResourcePrefix, TableIndex)))
            {
                _tableDocument.Load(stream);
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
            if(LoadJson() && EnsureTableIndex())
            {                
                result = true;
            }
            var message = result ? "End Converting Metadata" : "End Converting Metadata with errors";
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
            return result;
        }

        private bool EnsureTableIndex()
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
                _tableDocument.DocumentElement.SetAttribute("xmlns", "http://www.sa.dk/xmlns/diark/1.0");
                _tableDocument.Save(string.Format("{0}\\{1}", path, TableIndex));
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

            var node = CreateNode(_tableDocument, _tableDocument.SelectSingleNode("//tables"), TableNode);
            var tableName = tableInfo["name"].ToString();
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
            var node = CreateNode(_tableDocument, tableNode.SelectSingleNode("columns"), ColumnNode);
            var columnName = variableInfo["name"].ToString();
            var columnType = GetMappedType(variableInfo);
            node.SelectSingleNode("name").InnerText = columnName;
            node.SelectSingleNode("columnID").InnerText = string.Format(ColumnIDPrefix, index);
            node.SelectSingleNode("typeOriginal").InnerText = variableInfo["format"].ToString();
            node.SelectSingleNode("nullable").InnerText = variableInfo["nullable"].ToString().ToLower();
            node.SelectSingleNode("description").InnerText = variableInfo["description"].ToString();
            node.SelectSingleNode("type").InnerText = columnType;
            if((bool)variableInfo["isKey"])
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add key: {0} ", columnName) });
                var primaryKeyNode = CreateNode(_tableDocument, tableNode.SelectSingleNode("primaryKey"), PrimaryKeyColumnNode);
                primaryKeyNode.InnerText = columnName;
            }
            if (variableInfo["codeListKey"] != null && !string.IsNullOrEmpty(variableInfo["codeListKey"].ToString()))
            {
                AddReferencedTable(variableInfo, tableNode, index, columnType);
            }
        }
        
        private void AddReferencedTable(Dictionary<string, object> variableInfo, XmlNode tableNode, int index, string columnType)
        {
            _tablesCounter++;
            var tableName = tableNode.SelectSingleNode("name").InnerText;
            var codeListKey = variableInfo["codeListKey"].ToString();
            var refTableName = string.Format(ReferencedTable, tableName, codeListKey);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add referenced Table: {0} ", refTableName) });

            var node = CreateNode(_tableDocument, tableNode.SelectSingleNode("foreignKeys"), ForeignKeyNode);
            node.SelectSingleNode("name").InnerText = string.Format(ForeignKeyPrefix, tableName, codeListKey, index);
            node.SelectSingleNode("referencedTable").InnerText = refTableName;
            node.SelectSingleNode("reference/column").InnerText = variableInfo["name"].ToString();
           
            var refTableNode = CreateNode(_tableDocument, tableNode.ParentNode, ReferencedTableNode);
            refTableNode.SelectSingleNode("name").InnerText = refTableName;
            refTableNode.SelectSingleNode("folder").InnerText = string.Format(TableFolderPrefix, _tablesCounter);
            refTableNode.SelectSingleNode("description").InnerText = string.Format(ReferencedTableDescription, tableName);
            refTableNode.SelectSingleNode("primaryKey/name").InnerText = string.Format(PrimaryKeyPrefix, refTableName);
            var columnNodes = refTableNode.SelectNodes("columns/column");
            columnNodes[0].SelectSingleNode("type").InnerText = columnType;
            columnNodes[1].SelectSingleNode("type").InnerText = ParseOptions((object[])variableInfo["options"], refTableNode);
        }

        private string ParseOptions(object[] options, XmlNode refTableNode)
        {
            var result = 0;
            refTableNode.SelectSingleNode("rows").InnerText = options.Length.ToString();
            foreach (var option in options)
            {
                var length = ((Dictionary<string, object>)option)["description"].ToString().Length;
                if(length > result) { result = length; }
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
