using log4net;
using Rigsarkiv.Styx.Entities;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

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
        protected const string DataTypeIntPattern = "^(int)$|^(\\%([0-9]+)\\.0f)$|^(f([0-9]+)\\.)$|^(f([0-9]+))$";
        protected const string DataTypeDecimalPattern = "^(decimal)$|^(\\%([0-9]+)\\.([0-9]+)f)$|^(f([0-9]+)\\.([0-9]+))$|^(f([0-9]+)\\.([0-9]+))$";
        protected const string DataTypeDatePattern = "^([0-9]{4,4})-([0-9]{2,2})-([0-9]{2,2})$";
        protected const string DataTypeDateTimePattern = "^([0-9]{4,4})-([0-9]{2,2})-([0-9]{2,2})T([0-9]{2,2}):([0-9]{2,2}):([0-9]{2,2})$";
        protected delegate void OperationOnRow(XElement row);
        protected Assembly _assembly = null;
        protected Asta.Logging.LogManager _logManager = null;
        protected Dictionary<string, Regex> _regExps = null;
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
            _regExps = new Dictionary<string, Regex>();
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

        /// <summary>
        /// Get Converted column Value
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <param name="hasError"></param>
        /// <param name="isDifferent"></param>
        /// <returns></returns>
        protected string GetConvertedValue(Column column, string value, out bool hasError, out bool isDifferent)
        {
            string result = null;
            switch (column.TypeOriginal)
            {
                case "INTEGER": result = GetIntegerValue(column, value, out hasError, out isDifferent); break;
                case "DECIMAL": result = GetDecimalValue(column, value, out hasError, out isDifferent); break;
                case "DATE": result = GetDateValue(column, value, out hasError, out isDifferent); break;
                case "TIME": result = GetTimeValue(column, value, out hasError, out isDifferent); break;
                case "TIMESTAMP": result = GetTimeStampValue(column, value, out hasError, out isDifferent); break;
                default: result = GetStringValue(column, value, out hasError, out isDifferent); break;
            }
            return result;
        }

        private string GetTimeStampValue(Column column, string value, out bool hasError, out bool isDifferent)
        {
            hasError = false;
            isDifferent = false;
            return value;
        }

        private string GetDateValue(Column column, string value, out bool hasError, out bool isDifferent)
        {
            hasError = false;
            isDifferent = false;
            return value;
        }

        private string GetTimeValue(Column column, string value, out bool hasError, out bool isDifferent)
        {
            hasError = false;
            isDifferent = false;
            return value;
        }

        private string GetDecimalValue(Column column, string value, out bool hasError, out bool isDifferent)
        {
            isDifferent = false;
            var result = value;
            if (!_regExps.ContainsKey(DataTypeDecimalPattern))
            {
                _regExps.Add(DataTypeDecimalPattern, new Regex(DataTypeDecimalPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            }
            hasError = !_regExps[DataTypeDecimalPattern].IsMatch(column.Type);
            if (!hasError)
            {
                var intLength = 0;
                var decLength = 0;
                foreach (Group group in _regExps[DataTypeDecimalPattern].Match(column.Type).Groups)
                {
                    if (intLength == 0)
                    {
                        int.TryParse(group.Value, out intLength);
                    }
                    else
                    {
                        if (decLength == 0) { int.TryParse(group.Value, out decLength); }
                    }
                }                    
                if (intLength > 0 && decLength > 0)
                {
                    hasError = result.Length > (intLength + decLength + 2);
                    if (hasError) { result = result.Substring(0, intLength + decLength + 2); }
                }
            }
            isDifferent = result != value;
            return result;
        }

        private string GetIntegerValue(Column column, string value, out bool hasError, out bool isDifferent)
        {
            isDifferent = false;
            var result = value;
            if (!_regExps.ContainsKey(DataTypeIntPattern))
            {
                _regExps.Add(DataTypeIntPattern, new Regex(DataTypeIntPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            }
            hasError = !_regExps[DataTypeIntPattern].IsMatch(column.Type);
            if (!hasError)
            {
                var length = 0;
                foreach(Group group in _regExps[DataTypeIntPattern].Match(column.Type).Groups)
                {
                    if(int.TryParse(group.Value,out length)) { break; }
                }
                if(length > 0)
                {
                    hasError = result.Length > (length + 1);
                    if (hasError) { result = result.Substring(0, length + 1); }
                }
            }
            isDifferent = result != value;
            return result;
        }

        private string GetStringValue(Column column, string value, out bool hasError, out bool isDifferent)
        {
            hasError = false;
            isDifferent = false;
            var result = value;
            if (result.IndexOf("\"") > -1)
            {
                 result = string.Format("\"{0}\"", result.Replace("\"", "\"\""));
                 isDifferent = true;
            }
            result = result.Trim();
            if (!isDifferent) { isDifferent = result != value; }
            return result;
        }

        /// <summary>
        /// Stream xml rows by delegates
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="filePath"></param>
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
