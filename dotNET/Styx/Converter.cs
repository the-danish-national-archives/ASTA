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
        protected const string CodeListPath = "{0}\\Data\\{1}_{2}\\{1}_{2}_KODELISTE.txt";
        protected const string UserCodesPath = "{0}\\Data\\{1}_{2}\\{1}_{2}_BRUGERKODE.txt";
        protected const string TableDataPath = "{0}\\Data\\{1}_{2}\\{1}_{2}.csv";
        protected const string C1 = "c1";
        protected const string C2 = "c2";
        protected const string VarCharPrefix = "VARCHAR(";
        protected const string DataTypeIntPattern = "^(int)$|^(\\%([0-9]+)\\.0f)$|^(f([0-9]+)\\.)$|^(f([0-9]+))$";
        protected const string DataTypeDecimalPattern = "^(decimal)$|^(\\%([0-9]+)\\.([0-9]+)f)$|^(f([0-9]+)\\.([0-9]+))$|^(f([0-9]+)\\.([0-9]+))$";
        protected delegate void OperationOnRow(XElement row);
        protected Assembly _assembly = null;
        protected Asta.Logging.LogManager _logManager = null;
        protected Dictionary<string, Regex> _regExps = null;
        protected XNamespace _tableIndexXNS = TableIndexXmlNs;
        protected Report _report = null;
        protected XDocument _researchIndexXDocument = null;
        protected XDocument _tableIndexXDocument = null;
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
            _report = new Report() { Tables = new List<Table>() };
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

        protected Regex GetRegex(string pattern)
        {
            if (!_regExps.ContainsKey(pattern))
            {
                _regExps.Add(pattern, new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            }
            return _regExps[pattern];
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

        protected string GetColumnType(Column column)
        {
            var result = column.TypeOriginal;
            switch (column.TypeOriginal)
            {
                case "INTEGER": result = GetIntegerType(column); break;
                case "DECIMAL": result = GetDecimalType(column); break;
                case "DATE": result = GetDateType(column); break;
                case "TIME": result = GetTimeType(column); break;
                case "TIMESTAMP": result = GetTimeStampType(column); break;
                default: result = GetStringType(column); break;
            }
            return result;
        }

        private string GetTimeStampType(Column column)
        {
            var result = column.TypeOriginal;
            switch (_report.ScriptType)
            {
                case ScriptType.SPSS: result = "datetime20"; break;
                case ScriptType.SAS: result = "e8601dt."; break;
                case ScriptType.Stata: result = "%tcCCYY-NN-DD!THH:MM:SS"; break;
                case ScriptType.Xml: result = "datetime"; break;
            }
            return result;
        }

        private string GetDateType(Column column)
        {
            var result = column.TypeOriginal;
            switch (_report.ScriptType)
            {
                case ScriptType.SPSS: result = "sdate10"; break;
                case ScriptType.SAS: result = "yymmdd."; break;
                case ScriptType.Stata: result = "%tdCCYY-NN-DD"; break;
                case ScriptType.Xml: result = "date"; break;
            }
            return result;
        }

        private string GetTimeType(Column column)
        {
            var result = column.TypeOriginal;
            switch (_report.ScriptType)
            {
                case ScriptType.SPSS: result = "time8"; break;
                case ScriptType.SAS: result = "time."; break;
                case ScriptType.Stata: result = "%tcHH:MM:SS"; break;
                case ScriptType.Xml: result = "time"; break;
            }
            return result;
        }

        private string GetDecimalType(Column column)
        {
            var result = column.TypeOriginal;
            var lengths = GetDecimalLength(column);
            switch (_report.ScriptType)
            {
                case ScriptType.SPSS: result = string.Format("f{0}.{1}", lengths[0], lengths[1]); break;
                case ScriptType.SAS: result = string.Format("f{0}.{1}", lengths[0], lengths[1]); break;
                case ScriptType.Stata: result = string.Format("%{0}.{1}f", lengths[0], lengths[1]); break;
                case ScriptType.Xml: result = "decimal"; break;
            }
            return result;
        }

        private string GetIntegerType(Column column)
        {
            var result = column.TypeOriginal;
            var length = GetIntegerLength(column);
            switch (_report.ScriptType)
            {
                case ScriptType.SPSS: result = string.Format("f{0}", length); break;
                case ScriptType.SAS: result = string.Format("f{0}.", length); break;
                case ScriptType.Stata: result = string.Format("%{0}.0f", length); break;
                case ScriptType.Xml: result = "int"; break;
            }
            return result;
        }

        private string GetStringType(Column column)
        {
            var result = column.TypeOriginal;            
            var length = result.Substring(VarCharPrefix.Length);
            length = length.Substring(0, length.Length - 1);
            switch (_report.ScriptType)
            {
                case ScriptType.SPSS: result = string.Format("a{0}", length); break;
                case ScriptType.SAS: result = string.Format("${0}.", length); break;
                case ScriptType.Stata: result = string.Format("%{0}s", length); break;
                case ScriptType.Xml: result = "string"; break;
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
            var result = value.Replace(".", ",");
            var lengths = GetDecimalLength(column);
            hasError = (lengths[0] == 0 && lengths[1] == 0);
            if (!hasError)
            {
                if (column.MissingValues != null && column.MissingValues.ContainsKey(result))
                {
                    result = column.MissingValues[result];
                }
                if (lengths[0] > 0 && lengths[1] > 0)
                {
                    hasError = result.Length > (lengths[0] + lengths[1] + 2);
                    if (hasError) { result = result.Substring(0, lengths[0] + lengths[1] + 2); }
                }
            }
            isDifferent = result != value;
            return result;
        }

        protected int[] GetDecimalLength(Column column)
        {
            var result = new int[2] { 0, 0 };
            var regex = GetRegex(DataTypeDecimalPattern);            
            if(regex.IsMatch(column.Type))
            {
               var groups = regex.Match(column.Type).Groups;
               foreach (Group group in groups)
                {
                    if (result[0] == 0)
                    {
                        int.TryParse(group.Value, out result[0]);
                    }
                    else
                    {
                        if (result[1] == 0) { int.TryParse(group.Value, out result[1]); }
                    }
                }
            }
            return result;
        }

        private string GetIntegerValue(Column column, string value, out bool hasError, out bool isDifferent)
        {
            isDifferent = false;
            var result = value;           
            var length = GetIntegerLength(column);
            hasError = length == 0;
            if (!hasError)
            {
                if (column.MissingValues != null && column.MissingValues.ContainsKey(result))
                {
                    result = column.MissingValues[result];
                }
                hasError = result.Length > (length + 1);
                if (hasError) { result = result.Substring(0, length + 1); }
            }
            isDifferent = result != value;
            return result;
        }

        protected int GetIntegerLength(Column column)
        {
            var result = 0;
            var regex = GetRegex(DataTypeIntPattern);
            if(regex.IsMatch(column.Type))
            {
                var groups = regex.Match(column.Type).Groups;
                foreach (Group group in groups)
                {
                    if (int.TryParse(group.Value, out result)) { break; }
                }
            }
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
            if (result.IndexOf(";") > -1 && result.IndexOf("\"") == -1)
            {
                result = string.Format("\"{0}\"", result);
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
