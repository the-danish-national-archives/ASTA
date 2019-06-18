using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena.Logging;
using System.Collections.Generic;
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
            foreach (XmlNode node in _researchIndexDocument.SelectNodes("//diark:table", _researchIndexNS))
            {
                id = node.SelectSingleNode("diark:tableID", _researchIndexNS).InnerText;
                name = _tableIndexDocument.SelectSingleNode(string.Format("//diark:table[diark:folder = '{0}']/diark:name", id), _tableIndexNS).InnerText;
                ids.Add(id);
                result.Add(new Table() { Name = name, Folder = id });
            }
            result.ForEach(t => t.CodeList = GetCodeTables(t.Folder, ids));
            return result;
        }

        private List<Table> GetCodeTables(string folder, List<string> excludeIds)
        {
            List<Table> result = new List<Table>();
            string id = null;
            string name = null;
            foreach (XmlNode node in _tableIndexDocument.SelectNodes(string.Format("//diark:table[diark:folder = '{0}']/diark:foreignKeys/diark:foreignKey/diark:referencedTable", folder), _tableIndexNS))
            {
                name = node.InnerText;
                id = _tableIndexDocument.SelectSingleNode(string.Format("//diark:table[diark:name = '{0}']/diark:folder", name), _tableIndexNS).InnerText;
                if(!excludeIds.Contains(id))
                {
                    result.Add(new Table() { Name = name, Folder = id });
                }
            }            
            return result;
        }
    }
}
