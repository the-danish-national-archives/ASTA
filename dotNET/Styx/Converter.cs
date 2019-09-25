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
        protected const string ResourceScriptFile = "Rigsarkiv.Styx.Resources.{0}_import.{1}";
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
        protected const string EnclosedReservedWordPattern = "^(\")(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)(\")$";
        protected const string DataTypeDatetimePattern = "^([0-9]{4,4})-([0-9]{2,2})-([0-9]{2,2})T([0-9]{2,2}):([0-9]{2,2}):([0-9]{2,2})$";
        protected delegate void OperationOnRow(XElement row);
        protected Assembly _assembly = null;
        protected Asta.Logging.LogManager _logManager = null;
        protected Dictionary<string, Regex> _regExps = null;
        protected XNamespace _tableIndexXNS = TableIndexXmlNs;
        protected Report _report = null;
        protected XDocument _researchIndexXDocument = null;
        protected XDocument _tableIndexXDocument = null;
        protected Regex _enclosedReservedWord = null;
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
            _report = new Report() { Tables = new List<Table>(), ContextDocuments = new Dictionary<string, string>(), TablesCounter = 0, CodeListsCounter = 0 };
            _enclosedReservedWord = new Regex(EnclosedReservedWordPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
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

        protected string GetScriptTemplate()
        {
            string result = null;
            string resourceName = null;
            switch (_report.ScriptType)
            {
                case ScriptType.SPSS: resourceName = string.Format(ResourceScriptFile, _report.ScriptType.ToString().ToLower(),"sps"); break;
                case ScriptType.SAS: string.Format(ResourceScriptFile, _report.ScriptType.ToString().ToLower(), "sas"); break;
                case ScriptType.Stata: string.Format(ResourceScriptFile, _report.ScriptType.ToString().ToLower(), "do"); break;
            }
            using (Stream stream = _assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        protected string NormalizeName(string name)
        {
            var result = name;
            if (_enclosedReservedWord.IsMatch(name))
            {
                var groups = _enclosedReservedWord.Match(name).Groups;
                result = groups[2].Value;
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

        private string GetMonth(string monthValue)
        {
            string result = null;
            switch (monthValue)
            {
                case "01": result = "Jan"; break;
                case "02": result = "Feb"; break;
                case "03": result = "Mar"; break;
                case "04": result = "Apr"; break;
                case "05": result = "May"; break;
                case "06": result = "Jun"; break;
                case "07": result = "Jul"; break;
                case "08": result = "Aug"; break;
                case "09": result = "Sep"; break;
                case "10": result = "Oct"; break;
                case "11": result = "Nov"; break;
                case "12": result = "Dec"; break;
            }
            return result;
        }

        private string GetTimeStampValue(Column column, string value, out bool hasError, out bool isDifferent)
        {
            var regex = GetRegex(DataTypeDatetimePattern);
            hasError = !regex.IsMatch(value);
            var result = value;            
            if (!hasError)
            {
                var groups = regex.Match(value).Groups;
                if (_report.ScriptType == ScriptType.SPSS) { result = string.Format("{0}-{1}-{2} {3}:{4}:{5}", groups[3].Value, GetMonth(groups[2].Value), groups[1].Value, groups[4].Value, groups[5].Value, groups[6].Value); }
            }
            isDifferent = result != value;
            return result;
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
            if (column.MissingValues != null && column.MissingValues.ContainsKey(result))
            {
                result = column.MissingValues[result];
            }
            result = result.Replace(".", ",");
            var lengths = GetDecimalLength(column);
            hasError = (lengths[0] == 0 && lengths[1] == 0);
            if (!hasError)
            {                
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
