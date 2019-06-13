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
        const string ColumnNode = "<column><name></name><columnID></columnID><type></type><typeOriginal></typeOriginal><nullable>false</nullable><description></description></column>";
        const string TableNode = "<table><name></name><folder></folder><description></description><columns></columns><primaryKey><name>PK_</name><column></column></primaryKey><foreignKeys></foreignKeys><rows></rows></table>";
        const string IndicesPath = "{0}\\Indices";
        const string TableIndex = "tableIndex.xml";
        const string TableFolderPrefix = "table{0}";
        const string ColumnIDPrefix = "c{0}";
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
            node.SelectSingleNode("name").InnerText = tableInfo["name"].ToString();
            node.SelectSingleNode("folder").InnerText = folder;
            node.SelectSingleNode("rows").InnerText = tableInfo["rows"].ToString();
            node.SelectSingleNode("description").InnerText = tableInfo["description"].ToString();
            foreach (var variable in (object[])tableInfo["variables"])
            {
                var variableInfo = (Dictionary<string, object>)variable;
                AddColumnNode(variableInfo,node,index++);
            }
        }

        private void AddColumnNode(Dictionary<string, object> variableInfo,XmlNode tableNode,int index)
        {
            var node = CreateNode(_tableDocument, tableNode.SelectSingleNode("columns"), ColumnNode);
            node.SelectSingleNode("name").InnerText = variableInfo["name"].ToString();
            node.SelectSingleNode("columnID").InnerText = string.Format(ColumnIDPrefix, index);
            node.SelectSingleNode("typeOriginal").InnerText = variableInfo["format"].ToString();
            node.SelectSingleNode("description").InnerText = variableInfo["description"].ToString();
            node.SelectSingleNode("type").InnerText = GetMappedType(variableInfo);
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
