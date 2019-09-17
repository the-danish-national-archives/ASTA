using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Styx.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        const string TablePath = "{0}\\Tables\\{1}\\{1}.xml";
        const string CodeFormat = "'{0}' '{1}'";
        const string SpecialNumericPattern = "^(\\.[a-z])|([A-Z])$";
        const string SasSpecialNumericPattern = "^[A-Z]$";
        const string StataSpecialNumericPattern = "^\\.[a-z]$";
        const string Alphabet = "abcdefghijklmnopqrstuvwxyz";
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
        public Data(LogManager logManager, string srcPath, string destPath, string destFolder, Report report) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Data";
            _report = report;
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

        private bool EnsureTables()
        {
            var result = true;
            string path = null;
            try
            {
                 _report.Tables.ForEach(table =>
                {
                    XNamespace tableNS = string.Format(TableXmlNs, table.SrcFolder);
                    path = string.Format(TableDataPath, _destFolderPath, _report.ScriptType.ToString().ToLower(), table.Name);
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add file: {0}", path) });
                    using (TextWriter sw = new StreamWriter(path))
                    {
                        _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Write {0} data header", table.Folder) });
                        sw.WriteLine(string.Join(Separator, table.Columns.Select(c => c.Name).ToArray()));
                        path = string.Format(TablePath, _srcPath, table.SrcFolder);
                        StreamElement(delegate (XElement row) {
                            sw.WriteLine(GetRow(table, row, tableNS));
                            table.RowsCounter++;
                            if ((table.RowsCounter % RowsChunk) == 0) { _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("{0} of {1} rows added", table.RowsCounter, table.Rows) }); }
                        }, path);
                    }
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
            table.Columns.Where(c => c.CodeList != null).ToList().ForEach(column =>
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Ensure Missing Values for codelist: {0}", column.CodeList.Name) });
                XNamespace tableNS = string.Format(TableXmlNs, column.CodeList.SrcFolder);
                path = string.Format(TablePath, _srcPath, column.CodeList.SrcFolder);
                StreamElement(delegate (XElement row) {
                    var content = row.Element(tableNS + C1).Value;
                    if (column.MissingValues != null && !column.MissingValues.ContainsKey(content))
                    {
                        column.MissingValues.Add(content, content);
                    }
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
                        _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = string.Format("No new numric code available for column: {0}", column.Name) });
                    }
                    else
                    {
                        _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Map column {0} Missing Value {1} to {2}", column.Name, value.Key, newValue) });
                        column.MissingValues[value.Key] = newValue.ToString();
                        availableNumerics.Add(newValue.ToString());
                    }
                });
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
                    while (int.Parse(column.Lowest) > newValue && availableNumerics.Contains(newValue.ToString()))
                    {
                        newValue++;
                    }
                    if (newValue >= decimal.Parse(column.Lowest))
                    {
                        _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = string.Format("No new numric code available for column: {0}", column.Name) });
                    }
                    else
                    {
                        _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Map column {0} Missing Value {1} to {2}", column.Name, value.Key, newValue) });
                        column.MissingValues[value.Key] = newValue.ToString();
                        availableNumerics.Add(newValue.ToString());
                    }
                });
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
            var usercodes = new List<string>();
            table.Columns.Where(c => c.MissingValues != null).ToList().ForEach(column =>
            {   
                usercodes.Add(string.Join(" ", column.MissingValues.Select(v => string.Format("'{0}'", v.Value)).ToArray()));
            });
            if(usercodes.Count == 0) { return; }

            var path = string.Format(UserCodesPath, _destFolderPath, _report.ScriptType.ToString().ToLower(), table.Name);
            var content = File.ReadAllText(path);
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
                StreamElement(delegate (XElement row) {
                    var code = row.Element(tableNS + C1).Value;
                    if (column.MissingValues != null && column.MissingValues.ContainsKey(code))
                    {
                        code = column.MissingValues[code];
                    }
                    codeList.AppendLine(string.Format(CodeFormat, code, row.Element(tableNS + C2).Value));
                    table.RowsCounter++;
                }, path);
                var codeListContent = codeList.ToString();                
                _codeLists.Add(codeListContent.Substring(0, codeListContent.Length - 2));
                codeList.Clear();
            });
            path = string.Format(CodeListPath, _destFolderPath, _report.ScriptType.ToString().ToLower(), table.Name);
            var content = File.ReadAllText(path);
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.Write(string.Format(content, _codeLists.ToArray()));
            }
        }
    }
}
