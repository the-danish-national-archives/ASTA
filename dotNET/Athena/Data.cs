using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena.Logging;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// Data converter
    /// </summary>
    public class Data : Converter
    {
        private XmlNamespaceManager _researchIndexNS = null;
        private XmlNamespaceManager _tableIndexNS = null;
        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public Data(LogManager logManager, string srcPath, string destPath, string destFolder) : base(logManager, srcPath, destPath, destFolder)
        {
            var path = string.Format(IndicesPath, _destFolderPath);
            _researchIndexDocument.Load(string.Format("{0}\\{1}", path, ResearchIndex));
            _tableIndexDocument.Load(string.Format("{0}\\{1}", path, TableIndex));
            _researchIndexNS = new XmlNamespaceManager(_researchIndexDocument.NameTable);
            _tableIndexNS = new XmlNamespaceManager(_tableIndexDocument.NameTable);
            _researchIndexNS.AddNamespace("diark", TableIndexXmlNs);
            _tableIndexNS.AddNamespace("diark", TableIndexXmlNs);
        }

        /// <summary>
        /// start converter
        /// </summary>
        /// <returns></returns>
        public override bool Run()
        {
            return true;
        }

        public Row GetRow(Table table,int index)
        {
            var result = new Row() { DestValues = new Dictionary<string, string>(), SrcValues = new Dictionary<string, string>(), ErrorsColumns = new List<string>() } ;
            
            var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", table.Folder));
            if (File.Exists(path))
            {
                
                var tableDocument = new XmlDocument();
                tableDocument.Load(path);
                var tableNS = new XmlNamespaceManager(tableDocument.NameTable);
                tableNS.AddNamespace("tbns", string.Format(TableXmlNs, table.Folder));
                var rowNode = tableDocument.SelectSingleNode(string.Format("//tbns:row[{0}]", index), tableNS);
                table.Columns.ForEach(c =>
                {
                    var value = rowNode.SelectSingleNode(string.Format("tbns:{0}", c.Id), tableNS).InnerText;
                    var newValue = GetConvertedValue(c.Type, value);
                    result.SrcValues.Add(c.Id, value);
                    result.DestValues.Add(c.Id, newValue);
                });
            }
            else
            {
                //CSV file
            }
            return result;
        }

        private string GetConvertedValue(string type,string value)
        {
            switch (type)
            {
                case "INTEGER":
                    {
                        int result = -1;
                        int.TryParse(value, out result);
                        return result.ToString();
                    }; break;
                case "DECIMAL":
                    {
                        float result = -1;
                        float.TryParse(value, out result);
                        return result.ToString();
                    }; break;
                /*case "DATE": result = "DATE"; break;
                case "TIME": result = "TIME"; break;
                case "TIMESTAMP": result = "TIMESTAMP"; break;*/
                default:
                    {
                        return value;
                    }; break;
            }
        }

        /// <summary>
        /// Get Tables structure name/folder pair
        /// </summary>
        /// <returns></returns>
        public List<Table> GetTables()
        {
            List<Table> result = new List<Table>();
            List<string> ids = new List<string>();
            string id = null;
            string name = null;
            int rows = 0;
            foreach (XmlNode node in _researchIndexDocument.SelectNodes("//diark:table", _researchIndexNS))
            {
                id = node.SelectSingleNode("diark:tableID", _researchIndexNS).InnerText;
                XmlNode tableNode = _tableIndexDocument.SelectSingleNode(string.Format("//diark:table[diark:folder = '{0}']", id), _tableIndexNS);
                name = tableNode.SelectSingleNode("diark:name", _tableIndexNS).InnerText;
                rows = int.Parse(tableNode.SelectSingleNode("diark:rows", _tableIndexNS).InnerText);
                ids.Add(id);
                var table = new Table() { Name = name, Folder = id, Rows = rows, Columns = GetColumn(tableNode) };
                result.Add(table);
            }
            result.ForEach(t => t.CodeList = GetCodeTables(t.Folder, ids));
            return result;
        }

        private List<Table> GetCodeTables(string folder, List<string> excludeIds)
        {
            List<Table> result = new List<Table>();
            string id = null;
            string name = null;
            int rows = 0;
            foreach (XmlNode node in _tableIndexDocument.SelectNodes(string.Format("//diark:table[diark:folder = '{0}']/diark:foreignKeys/diark:foreignKey/diark:referencedTable", folder), _tableIndexNS))
            {
                name = node.InnerText;
                XmlNode tableNode = _tableIndexDocument.SelectSingleNode(string.Format("//diark:table[diark:name = '{0}']", name), _tableIndexNS);
                id = tableNode.SelectSingleNode("diark:folder", _tableIndexNS).InnerText;
                rows = int.Parse(tableNode.SelectSingleNode("diark:rows", _tableIndexNS).InnerText);
                if (!excludeIds.Contains(id))
                {
                    var table = new Table() { Name = name, Folder = id, Rows = rows, Columns = GetColumn(tableNode) };
                    result.Add(table);
                }
            }            
            return result;
        }

        private List<Column> GetColumn(XmlNode tableNode)
        {
            List<Column> result = new List<Column>();
            foreach (XmlNode node in tableNode.SelectNodes("diark:columns/diark:column", _tableIndexNS))
            {
                var column = new Column();
                column.Name = node.SelectSingleNode("diark:name", _tableIndexNS).InnerText;
                column.Id = node.SelectSingleNode("diark:columnID", _tableIndexNS).InnerText;
                column.Type = node.SelectSingleNode("diark:type", _tableIndexNS).InnerText;
                column.TypeOriginal = node.SelectSingleNode("diark:typeOriginal", _tableIndexNS).InnerText;
                result.Add(column);
            }
            return result;
        }
    }
}
