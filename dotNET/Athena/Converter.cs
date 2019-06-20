using Rigsarkiv.Athena.Logging;
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
        protected Assembly _assembly = null;
        protected LogManager _logManager = null;
        protected XDocument _tableIndexXDocument = null;
        protected XDocument _researchIndexXDocument = null;
        protected XmlWriter _writer = null;
        protected XmlDocument _tableIndexDocument = null;
        protected XmlDocument _researchIndexDocument = null;
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
            _tableIndexDocument = new XmlDocument();
            _researchIndexDocument = new XmlDocument();
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
