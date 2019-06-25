using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// Converter base class
    /// </summary>
    public class Converter
    {
        protected const string IndicesPath = "{0}\\Indices";
        protected const string ResourcePrefix = "Rigsarkiv.Athena.Resources.{0}";
        protected const string TableIndex = "tableIndex.xml";
        protected const string ResearchIndex = "researchIndex.xml";
        protected const string TableIndexXmlNs = "http://www.sa.dk/xmlns/diark/1.0";
        protected const string TableXmlNs = "http://www.sa.dk/xmlns/siard/1.0/schema0/{0}.xsd";
        protected const string TableSchemaLocation = "http://www.sa.dk/xmlns/siard/1.0/schema0/{0}.xsd {0}.xsd";
        protected const string TableXsiNs = "http://www.w3.org/2001/XMLSchema-instance";
        protected const string TablePath = "{0}\\Tables\\{1}";
        protected const string VarCharPrefix = "VARCHAR({0})";
        protected Assembly _assembly = null;
        protected LogManager _logManager = null;
        protected XDocument _tableIndexXDocument = null;
        protected XDocument _researchIndexXDocument = null;
        protected XNamespace _tableIndexXNS = TableIndexXmlNs;
        protected XmlWriter _writer = null;
        protected List<Table> _tables = null;
        protected string _srcPath = null;
        protected string _destPath = null;
        protected string _destFolder = null;
        protected string _logSection = "";
        protected string _destFolderPath = null;
        protected string _srcFolder = null;

        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public Converter(LogManager logManager, string srcPath, string destPath, string destFolder)
        {
            _assembly = Assembly.GetExecutingAssembly();
            _logManager = logManager;
            _tables = new List<Table>();
            _srcPath = srcPath;
            _destPath = destPath;
            _destFolder = destFolder;
            _destFolderPath = string.Format("{0}\\{1}", _destPath, _destFolder);
            var folderName = _srcPath.Substring(_srcPath.LastIndexOf("\\") + 1);
            _srcFolder = folderName.Substring(0, folderName.LastIndexOf("."));
        }

        /// <summary>
        /// start converter
        /// </summary>
        /// <returns></returns>
        public virtual bool Run()
        {
            return true;
        }

        /// <summary>
        /// Tables metadata
        /// </summary>
        public List<Table> Tables {
            get { return _tables; }            
        }

        /// <summary>
        /// Table Index XDocument
        /// </summary>
        public XDocument TableIndexXDocument
        {
            get { return _tableIndexXDocument; }
            set { _tableIndexXDocument = value; }
        }

        /// <summary>
        /// Research Index XDocument
        /// </summary>
        public XDocument ResearchIndexXDocument
        {
            get { return _researchIndexXDocument; }
            set { _researchIndexXDocument = value; }
        }

        protected int GetColumnLength(string type,string regExp)
        {
            var result = 0;
            var startIndex = -1;
            var endIndex = -1;
            switch (type)
            {
                case "INTEGER":
                    {
                        startIndex = regExp.IndexOf("{1,") + 3;
                        endIndex = regExp.IndexOf("}$");
                        result = int.Parse(regExp.Substring(startIndex, endIndex - startIndex));
                        result++;
                        if (result < 2) { result = 2; }                        
                    };
                    break;
                case "DECIMAL":
                    {
                        startIndex = regExp.IndexOf("{1,") + 3;
                        endIndex = regExp.IndexOf("}", startIndex);
                        result = int.Parse(regExp.Substring(startIndex, endIndex - startIndex));
                        result++;
                        startIndex = regExp.IndexOf("{1,", endIndex + 1);
                        if(startIndex > -1)
                        {
                            startIndex = startIndex + 3;
                            endIndex = regExp.LastIndexOf("}");
                            result += int.Parse(regExp.Substring(startIndex, endIndex - startIndex));
                            result++;
                        }
                        if (result < 2) { result = 2; }
                    };
                    break;
                case "VARCHAR({0})":
                    {
                        startIndex = regExp.IndexOf("{0,") + 3;
                        endIndex = regExp.IndexOf("}$");
                        result = int.Parse(regExp.Substring(startIndex, endIndex - startIndex));
                    };
                    break;
            }
            return result;
        }

        /// <summary>
        /// Add Missing Column Node
        /// </summary>
        /// <param name="codeName"></param>
        /// <param name="researchIndexNode"></param>
        /// <param name="columnId"></param>
        protected void AddMissingColumnNode(string codeName, XElement researchIndexNode, string columnId)
        {

            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add missing value: {0} ", codeName) });
            XElement columnsNode = researchIndexNode.Element(_tableIndexXNS + "columns");
            XElement columnNode = new XElement(_tableIndexXNS + "column", new XElement(_tableIndexXNS + "columnID", columnId), new XElement(_tableIndexXNS + "missingValues"));
            if (columnsNode == null)
            {
                columnsNode = new XElement(_tableIndexXNS + "columns", columnNode);
                researchIndexNode.Add(columnsNode);
            }
            if (!columnsNode.Elements().Where(e => e.Element(_tableIndexXNS + "columnID").Value == columnId).Any())
            {
                columnsNode.Add(columnNode);
            }
            else
            {
                columnNode = columnsNode.Elements().Where(e => e.Element(_tableIndexXNS + "columnID").Value == columnId).FirstOrDefault();
            }
            var missingValueNode = new XElement(_tableIndexXNS + "value", codeName);
            columnNode.Element(_tableIndexXNS + "missingValues").Add(missingValueNode);
        }

        /// <summary>
        /// start table writer
        /// </summary>
        /// <param name="folder"></param>
        protected void StartWriter(string folder)
        {
            var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", folder));
            var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };
            _writer = XmlWriter.Create(path, settings);
            _writer.WriteStartDocument();
            _writer.WriteStartElement("table", string.Format(TableXmlNs, folder));
            _writer.WriteAttributeString("xmlns", "xsi", null, TableXsiNs);
            _writer.WriteAttributeString("xsi", "schemaLocation", null, string.Format(TableSchemaLocation, folder));
        }

        /// <summary>
        /// end table writer
        /// </summary>
        protected void EndWriter()
        {
            _writer.WriteEndElement();
            _writer.WriteEndDocument();
            _writer.Flush();
            _writer.Dispose();
        }
    }
}
