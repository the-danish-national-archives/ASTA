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
        /// 
        /// </summary>
        /// <param name="isMain"></param>
        /// <returns></returns>
        public Dictionary<string,string> GetTables(bool isMain)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if(isMain)
            {
                foreach(XmlNode node in _researchIndexDocument.SelectNodes("//diark:table", _researchIndexNS))
                {
                    var id = node.SelectSingleNode("diark:tableID", _researchIndexNS).InnerText;
                    var name = _tableIndexDocument.SelectSingleNode(string.Format("//diark:table[diark:folder = '{0}']/diark:name", id), _tableIndexNS).InnerText;
                    result.Add(name, id);
                }
            }
            return result;
        }
    }
}
