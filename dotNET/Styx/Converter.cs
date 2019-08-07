using log4net;
using Rigsarkiv.Styx.Entities;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Rigsarkiv.Styx
{
    public class Converter
    {
        protected static readonly ILog _log = LogManager.GetLogger(typeof(Converter));
        protected const string ResourceLogFile = "Rigsarkiv.Styx.Resources.log.html";
        protected const string DataPath = "{0}\\Data";       
        protected const string TableIndexXmlNs = "http://www.sa.dk/xmlns/diark/1.0";
        protected const string TableXmlNs = "http://www.sa.dk/xmlns/siard/1.0/schema0/{0}.xsd";
        protected const string TableSchemaLocation = "http://www.sa.dk/xmlns/siard/1.0/schema0/{0}.xsd {0}.xsd";
        protected const string TableXsiNs = "http://www.w3.org/2001/XMLSchema-instance";
        protected const string CodeListPath = "{0}\\Data\\{1}\\{2}_KODELISTE.txt";
        protected const string TableDataPath = "{0}\\Data\\{1}\\{2}.csv";
        protected const string C1 = "c1";
        protected const string C2 = "c2";
        protected delegate void OperationOnRow(XElement row);
        protected Assembly _assembly = null;
        protected Asta.Logging.LogManager _logManager = null;
        protected XNamespace _tableIndexXNS = TableIndexXmlNs;
        protected Report _report = null;
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
        public Converter(Asta.Logging.LogManager logManager, string srcPath, string destPath, string destFolder)
        {
            _assembly = Assembly.GetExecutingAssembly();
            _logManager = logManager;
            _report = new Report() { ScriptType = ScriptType.SPSS, Tables = new List<Table>() };
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
        /// Report
        /// </summary>
        public Report Report
        {
            get { return _report; }
        }

        /// <summary>
        /// Get Log file Template
        /// </summary>
        /// <returns></returns>
        public string GetLogTemplate()
        {
            string result = null;
            using (Stream stream = _assembly.GetManifestResourceStream(ResourceLogFile))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }        

        protected void StreamElement(OperationOnRow operation, string filePath)
        {
            using (var rdr = XmlReader.Create(filePath))
            {
                rdr.MoveToContent();
                while (rdr.Read())
                {
                    if ((rdr.NodeType == XmlNodeType.Element) && (rdr.Name == "row"))
                    {
                        operation(XNode.ReadFrom(rdr) as XElement);
                    }
                }
                rdr.Close();
            }
        }
    }
}
