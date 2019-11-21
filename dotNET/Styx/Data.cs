using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Styx.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace Rigsarkiv.Styx
{
    /// <summary>
    /// Data converter
    /// </summary>
    public class Data : Converter
    {
        const int RowsChunk = 500;
        const string Separator = ";";
        const string ResourceReportFile = "Rigsarkiv.Styx.Resources.report.html";
        const string TablePath = "{0}\\Tables\\{1}\\{1}.xml";
        const string CodeFormat = "'{0}' '{1}'";
        const string SpecialNumericPattern = "^(\\.[a-z])|([A-Z])$";
        const string SasSpecialNumericPattern = "^[A-Z]$";
        const string StataSpecialNumericPattern = "^\\.[a-z]$";
        const string Alphabet = "abcdefghijklmnopqrstuvwxyz";
        const string UserCodeRange = "'{0}' 'through' '{1}'{2}";
        const string UserCodeExtra = " 'and' '{0}'";
        private List<string> _codeLists = null;
        private List<string> _sasSpecialNumerics = null;
        private List<string> _stataSpecialNumerics = null;

        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public Data(LogManager logManager, string srcPath, string destPath, string destFolder, Report report, FlowState state) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Data";
            _report = report;
            _state = state;
            _codeLists = new List<string>();
            _sasSpecialNumerics = new List<string>();
            _stataSpecialNumerics = new List<string>();
            foreach (char c in Alphabet)
            {
                _sasSpecialNumerics.Add(string.Format("{0}", c.ToString().ToUpper()));
                _stataSpecialNumerics.Add(string.Format(".{0}", c.ToString()));
            }
        }

        /// <summary>
        /// start converter
        /// </summary>
        /// <returns></returns>
        public override bool Run()
        {
            var result = false;
            var message = string.Format("Start Converting Data {0} -> {1}", _srcFolder, _destFolder);
            _log.Info(message);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
           if(EnsureMissingValues() && EnsureCodeLists() && EnsureTables())
            {
                result = true;
                if (_report.ScriptType == ScriptType.SPSS) { result = EnsureUserCodes(); }                
            }
            message = result ? "End Converting Data" : "End Converting Data with errors";
            _log.Info(message);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
            return result;
        }

        /// <summary>
        /// flush and save report file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public bool Flush(string path, string name)
        {
            var result = true;
            try
            {
                _log.Info("Flush report");
                var json = new JavaScriptSerializer().Serialize(_report);
                string data = GetReportTemplate();
                File.WriteAllText(path, string.Format(data, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), name, json));
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("Failed to Flush log", ex);
            }
            return result;
        }

        private bool EnsureTables()
        {
            var result = true;
            string path = null;
            try
            {
                 _report.Tables.ForEach(table =>
                {
                    XNamespace tableNS = string.Format(TableXmlNs, table.SrcFolder);
                    path = string.Format(TableDataPath, _destFolderPath, _report.ScriptType.ToString().ToLower(), _state == FlowState.Completed ? NormalizeName(table.Title) : NormalizeName(table.Name));
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add file: {0}", path) });
                    using (TextWriter sw = new StreamWriter(path))
                    {
                        _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Write {0} data header", table.Folder) });
                        sw.WriteLine(string.Join(Separator, table.Columns.Select(c => NormalizeName(c.Name)).ToArray()));
                        path = string.Format(TablePath, _srcPath, table.SrcFolder);
                        StreamElement(delegate (XElement row) {
                            sw.WriteLine(GetRow(table, row, tableNS));
                            table.RowsCounter++;
                            if ((table.RowsCounter % RowsChunk) == 0) { _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("{0} of {1} rows added", table.RowsCounter, table.Rows) }); }
                        }, path);                        
                    }
                    _report.TablesCounter++;
                });
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("EnsureTables Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureTables Failed: {0}", ex.Message) });
            }
            return result;
        }

        private string GetRow(Table table, XElement row, XNamespace tableNS)
        {
            var result = string.Empty;
            var contents = new List<string>();
            table.Columns.ForEach(column => {
                var hasError = false;
                var isDifferent = false;
                var content = row.Element(tableNS + column.Id).Value;
                if (!string.IsNullOrEmpty(content))
                {
                    content = GetConvertedValue(column, content, out hasError, out isDifferent);
                }
                contents.Add(content);
            });            
            return string.Join(Separator, contents.ToArray());
        }

        private void UpdateRange(Column column,string value)
        {
            if (string.IsNullOrEmpty(column.Highest)) { column.Highest = "0"; }
            if (string.IsNullOrEmpty(column.Lowest)) { column.Lowest = "0"; }

            if (column.TypeOriginal == "INTEGER")
            {
                int current = 0;
                if (int.TryParse(value, out current))
                {
                    var tmp = int.Parse(column.Highest);
                    if (current > tmp) { column.Highest = current.ToString(); }
                    tmp = int.Parse(column.Lowest);
                    if (current < tmp) { column.Lowest = current.ToString(); }
                }                
            }
            if (column.TypeOriginal == "DECIMAL")
            {
                var newValue = value.Replace(",", ".");
                decimal current = 0;
                if (decimal.TryParse(newValue, out current))
                {
                    var tmp = decimal.Parse(column.Highest);
                    if (current > tmp) { column.Highest = current.ToString(); }
                    tmp = decimal.Parse(column.Lowest);
                    if (current < tmp) { column.Lowest = current.ToString(); }
                }
            }
        }

        private void EnsureTableMissingValues(Table table, XNamespace tableNS)
        {
            var counter = 0;
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Ensure Missing Values for table: {0}", table.Name) });
            var path = string.Format(TablePath, _srcPath, table.SrcFolder);
            StreamElement(delegate (XElement row) {
                table.Columns.Where(c => c.MissingValues != null).ToList().ForEach(column => {
                    var content = row.Element(tableNS + column.Id).Value;
                    if (!string.IsNullOrEmpty(content))
                    {
                        UpdateRange(column, content);
                    }
                });
                counter++;
                if ((counter % RowsChunk) == 0) { _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("{0} of {1} rows checked", counter, table.Rows) }); }
            }, path);
        }

        private void EnsureCodeListMissingValues(Table table)
        {
            string path = null;
            table.Columns.Where(c => c.CodeList != null && c.MissingValues != null).ToList().ForEach(column =>
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Ensure Missing Values for codelist: {0}", column.CodeList.Name) });
                XNamespace tableNS = string.Format(TableXmlNs, column.CodeList.SrcFolder);
                path = string.Format(TablePath, _srcPath, column.CodeList.SrcFolder);
                var columnId = column.CodeList.Columns.Where(c => c.IsKey).Select(c => c.Id).FirstOrDefault();
                StreamElement(delegate (XElement row) {
                    var content = row.Element(tableNS + columnId).Value;
                    UpdateRange(column, content);
                }, path);
            });
        }

        private void ConvertMissingValuesToIntegers(Regex regex, Column column, int length, List<string> availableNumerics)
        {
            if (length > 0)
            {
                if (length > 9) { length = 9; }
                int newValue = ((int.Parse(Math.Pow(10, length).ToString()) - 1) * -1);
                column.MissingValues.Where(v => regex.IsMatch(v.Key)).ToList().ForEach(value =>
                {
                    while (int.Parse(column.Lowest) > newValue && availableNumerics.Contains(newValue.ToString()))
                    {
                        newValue++;
                    }
                    if (newValue >= int.Parse(column.Lowest))
                    {
                        column.Message = string.Format("No new numric code less than {0} available for column: {1}", column.Lowest, column.Name);
                        _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = column.Message });
                    }
                    else
                    {
                        _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Map column {0} Missing Value {1} to {2}", column.Name, value.Key, newValue) });
                        column.MissingValues[value.Key] = newValue.ToString();
                        availableNumerics.Add(newValue.ToString());
                    }
                });
                if(string.IsNullOrEmpty(column.Message))
                {
                    column.SortedMissingValues = column.MissingValues.Values.ToList();
                    column.SortedMissingValues.Sort(new IntComparer());
                }
            }
        }

        private void ConvertMissingValuesToDecimals(Regex regex, Column column, int length, List<string> availableNumerics)
        {
            if (length > 0)
            {
                if (length > 9) { length = 9; }
                decimal newValue = ((decimal.Parse(Math.Pow(10, length).ToString()) - 1) * -1);
                column.MissingValues.Where(v => regex.IsMatch(v.Key)).ToList().ForEach(value =>
                {
                    while (decimal.Parse(column.Lowest) > newValue && availableNumerics.Contains(newValue.ToString()))
                    {
                        newValue++;
                    }
                    if (newValue >= decimal.Parse(column.Lowest))
                    {
                        column.Message = string.Format("No new numric code less than {0} available for column: {1}", column.Lowest, column.Name);
                        _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = column.Message });
                    }
                    else
                    {
                        _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Map column {0} Missing Value {1} to {2}", column.Name, value.Key, newValue) });
                        column.MissingValues[value.Key] = newValue.ToString();
                        availableNumerics.Add(newValue.ToString());
                    }
                });
                if (string.IsNullOrEmpty(column.Message))
                {
                    column.SortedMissingValues = column.MissingValues.Values.ToList();
                    column.SortedMissingValues.Sort(new DecimalComparer());
                }
            }
        }

        private void ConvertMissingValuesToNumbers(Regex regex, Column column)
        {
            var availableNumerics = new List<string>();
            column.MissingValues.Where(v => !regex.IsMatch(v.Key)).ToList().ForEach(value => {
                if (!availableNumerics.Contains(value.Value)) { availableNumerics.Add(value.Value); }
            });
            int length = -1;
            if (column.TypeOriginal == "INTEGER")
            {
                length = GetIntegerLength(column);
                ConvertMissingValuesToIntegers(regex, column, length, availableNumerics);
            }
            if (column.TypeOriginal == "DECIMAL")
            {
                length = GetDecimalLength(column)[0];
                ConvertMissingValuesToDecimals(regex, column, length, availableNumerics);
            }            
        }

        private void ConvertMissingValuesToChars(Regex regex, Column column,string[] specialNumerics)
        {
            var result = true;
            var availableNumerics = new List<string>();
            availableNumerics.AddRange(specialNumerics);
            column.MissingValues.Where(v => regex.IsMatch(v.Value)).ToList().ForEach(value => {
                if(availableNumerics.Contains(value.Value)) { availableNumerics.Remove(value.Value); }
            });
            column.MissingValues.Where(v => !regex.IsMatch(v.Value)).ToList().ForEach(value =>
            {
                result = availableNumerics.Count > 0;
                if(result)
                {
                    var newValue = availableNumerics[0];
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Map column {0} Missing Value {1} to {2}", column.Name, value.Key, newValue) });
                    column.MissingValues[value.Key] = newValue;
                    availableNumerics.Remove(newValue);
                }                    
            });
            if (!result)
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = string.Format("No new Special Numeric code available for column: {0}", column.Name) });
            }
        }

        private bool EnsureMissingValues()
        {
            var result = true;            
            try
            {
                Regex regex = null;
                switch(_report.ScriptType)
                {
                    case ScriptType.SPSS: regex = GetRegex(SpecialNumericPattern);break;
                    case ScriptType.SAS: regex = GetRegex(SasSpecialNumericPattern); break;
                    case ScriptType.Stata: regex = GetRegex(StataSpecialNumericPattern); break;
                }                
                _report.Tables.ForEach(table =>
                {
                    if (_report.ScriptType == ScriptType.SPSS)
                    {
                        EnsureTableMissingValues(table, string.Format(TableXmlNs, table.SrcFolder));
                        EnsureCodeListMissingValues(table);
                    }                   
                    table.Columns.Where(c => c.MissingValues != null).ToList().ForEach(column =>
                    {
                        if (_report.ScriptType == ScriptType.SPSS) { ConvertMissingValuesToNumbers(regex, column); }
                        if (_report.ScriptType == ScriptType.SAS) { ConvertMissingValuesToChars(regex, column,_sasSpecialNumerics.ToArray()); }
                        if (_report.ScriptType == ScriptType.Stata) { ConvertMissingValuesToChars(regex, column, _stataSpecialNumerics.ToArray()); }
                    });
                });
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("EnsureMissingValues Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureMissingValues Failed: {0}", ex.Message) });
            }
            return result;
        }

        private bool EnsureUserCodes()
        {
            var result = true;
            try
            {
                _report.Tables.ForEach(table =>
                {
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Write {0} user codes", table.Folder) });
                    EnsureUserCode(table);
                });
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("EnsureUserCodes Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureUserCodes Failed: {0}", ex.Message) });
            }
            return result;
        }

        private void EnsureUserCode(Table table)
        {
            string content = null;
            var usercodes = new List<string>();
            table.Columns.Where(c => c.MissingValues != null).ToList().ForEach(column =>
            {
                content = string.Empty;
                if (column.SortedMissingValues != null)
                {
                    if (column.SortedMissingValues.Count < 4)
                    {
                        content = string.Join(" ", column.SortedMissingValues.Select(v => string.Format("'{0}'", v)).ToArray());
                    }
                    else
                    {
                        _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Apply User codes range for column {0}", column.Name) });
                        string lastValue = column.SortedMissingValues[0];
                        var lastIndex = 1;
                        column.SortedMissingValues.ForEach(v => {
                            if ((column.TypeOriginal == "INTEGER" && int.Parse(v) == (int.Parse(lastValue) + 1)) || (column.TypeOriginal == "DECIMAL" && decimal.Parse(v) == (decimal.Parse(lastValue) + 1)))
                            {
                                lastValue = v;
                                lastIndex++;
                            }
                        });
                        if (column.SortedMissingValues.Count > (lastIndex + 1)) { column.Message = "out of range"; }
                        content = string.Format(UserCodeRange, column.SortedMissingValues[0], lastValue, lastIndex < column.SortedMissingValues.Count ? string.Format(UserCodeExtra, column.SortedMissingValues[lastIndex]) : string.Empty);
                    }
                }                
                usercodes.Add(content);
            });
            if(usercodes.Count == 0) { return; }

            var path = string.Format(UserCodesPath, _destFolderPath, _report.ScriptType.ToString().ToLower(), NormalizeName(table.Name));
            content = File.ReadAllText(path);
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.Write(string.Format(content, usercodes.ToArray()));
            }
        }

        private bool EnsureCodeLists()
        {
            var result = true;
            try
            {
                _report.Tables.ForEach(table =>
                {
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Write {0} code lists", table.Folder) });
                    _codeLists.Clear();
                    if (table.Columns.Any(c => c.CodeList != null))
                    {
                        EnsureCodeList(table);                        
                    }
                });
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("EnsureCodeLists Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureCodeLists Failed: {0}", ex.Message) });
            }
            return result;
        }

        private void EnsureCodeList(Table table)
        {
            string path = null;            
            var codeList = new StringBuilder();
            table.Columns.Where(c => c.CodeList != null).ToList().ForEach(column =>
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add {0} code list", column.CodeList.Name) });
                XNamespace tableNS = string.Format(TableXmlNs, column.CodeList.SrcFolder);
                path = string.Format(TablePath, _srcPath, column.CodeList.SrcFolder);
                var columnKeyId = column.CodeList.Columns.Where(c => c.IsKey).Select(c => c.Id).FirstOrDefault();
                var columnId = column.CodeList.Columns.Where(c => !c.IsKey).Select(c => c.Id).FirstOrDefault();
                StreamElement(delegate (XElement row) {
                    var code = row.Element(tableNS + columnKeyId).Value;
                    if (column.MissingValues != null && column.MissingValues.ContainsKey(code))
                    {
                        code = column.MissingValues[code];
                    }
                    codeList.AppendLine(string.Format(CodeFormat, code, row.Element(tableNS + columnId).Value));
                    column.CodeList.RowsCounter++;
                }, path);
                var codeListContent = codeList.ToString();                
                _codeLists.Add(codeListContent.Substring(0, codeListContent.Length - 2));
                codeList.Clear();
                _report.CodeListsCounter++;
            });
            path = string.Format(CodeListPath, _destFolderPath, _report.ScriptType.ToString().ToLower(), NormalizeName(table.Name));
            var content = File.ReadAllText(path);
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.Write(string.Format(content, _codeLists.ToArray()));
            }            
        }

        private string GetReportTemplate()
        {
            string result = null;
            using (Stream stream = _assembly.GetManifestResourceStream(ResourceReportFile))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }
    }
}
