using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// Data converter
    /// https://lennilobel.wordpress.com/2009/09/02/streaming-into-linq-to-xml-using-c-custom-iterators-and-xmlreader/
    /// </summary>
    public class Data : Converter
    {
        const char Separator = ';';
        const string SpecialNumericPattern = "^\\.[a-zA-Z]$";
        const string DoubleApostrophePattern = "^\"([\\w\\W\\s]*)\"$";
        private Dictionary<string, Regex> _regExps = null;
        private bool _updateDocuments = false;

        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        /// <param name="tables"></param>
        public Data(LogManager logManager, string srcPath, string destPath, string destFolder, List<Table> tables) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Data";
            _tables = tables;
            _regExps = new Dictionary<string, Regex>();
        }

        /// <summary>
        /// start converter
        /// </summary>
        /// <returns></returns>
        public override bool Run()
        {
            var result = false;
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Start Converting Data {0} -> {1}", _srcFolder, _destFolder) });
            if (_tables != null && _tables.Count > 0)
            {
                _tables.ForEach(t => {
                    if(!AddFile(t)) { result = false; }
                });
                if(_updateDocuments)
                {
                    var path = string.Format(IndicesPath, _destFolderPath);
                    _tableIndexXDocument.Save(string.Format("{0}\\{1}", path, TableIndex));
                    _researchIndexXDocument.Save(string.Format("{0}\\{1}", path, ResearchIndex));
                }
                result = true;
            }
            else
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = "Tables metadata Property is empty" });
            }
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = (result ? "End Converting Data" : "End Converting Data with errors") });
            return result;
        }

        /// <summary>
        /// Get spcified indexed Row 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Row GetRow(Table table,int index)
        {
            Row result = null;
            var xmlPath = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", table.Folder));
            var csvPath = string.Format("{0}\\Data\\{1}\\{1}.csv", _srcPath.Substring(0, _srcPath.LastIndexOf(".")), table.SrcFolder);
            if (File.Exists(xmlPath))
            {
                XNamespace tableNS = string.Format(TableXmlNs, table.Folder);
                XElement rowNode = StreamElement(xmlPath, index);
                List<string> rowLine = File.Exists(csvPath) ? StreamLine(csvPath, index) : null;
                if (rowLine == null && !table.Errors.HasValue)
                {
                    table.Errors = 0;
                    table.ErrorsRows = new List<int>();
                }
                if (rowNode != null)
                {
                    var counter = 0;
                    var hasError = false;
                    result = new Row() { DestValues = new Dictionary<string, string>(), SrcValues = new Dictionary<string, string>(), ErrorsColumns = new List<string>() };
                    table.Columns.ForEach(c =>
                    {
                        UpdateRow(table, result, tableNS, rowNode, rowLine, c, index, counter, hasError);
                        counter++;
                    });
                    if (rowLine == null && hasError)
                    {
                        table.Errors++;
                        table.ErrorsRows.Add(index);
                    }
                }
            }
            else
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("None Exist file: {0}", xmlPath) });
            }
            return result;
        }

        private void UpdateRow (Table table,Row row, XNamespace tableNS,XElement rowNode, List<string> rowLine, Column column,int index, int counter,bool hasError)
        {
            string value = null;
            string newValue = null;
            if (rowLine != null)
            {
                value = rowLine[counter];
                if (string.IsNullOrEmpty(value.Trim()) && column.Nullable) { value = string.Empty; }
                if (table.ErrorsRows.Contains(index)) { GetConvertedValue(column, value, out hasError); }
                newValue = rowNode.Element(tableNS + column.Id).Value;
            }
            else
            {
                value = rowNode.Element(tableNS + column.Id).Value;
                if (string.IsNullOrEmpty(value.Trim()) && column.Nullable) { value = string.Empty; }
                newValue = GetConvertedValue(column, value, out hasError);                
            }
            row.SrcValues.Add(column.Id, value);
            row.DestValues.Add(column.Id, newValue);
            if (hasError) { row.ErrorsColumns.Add(column.Id); }
        }

        private bool AddFile(Table table)
        {
            var result = true;
            try
            {
                var researchIndexNode = _researchIndexXDocument.Element(_tableIndexXNS + "researchIndex").Element(_tableIndexXNS + "mainTables").Elements().Where(e => e.Element(_tableIndexXNS + "tableID").Value == table.Folder).FirstOrDefault();
                var tableNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "folder").Value == table.Folder).FirstOrDefault();
                var counter = 1;
                var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", table.Folder));
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add file: {0} ", path) });
                StartWriter(table.Folder);
                path = string.Format("{0}\\Data\\{1}\\{1}.csv", _srcPath.Substring(0, _srcPath.LastIndexOf(".")), table.SrcFolder);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Loop file: {0} ", path) });
                if (!table.Errors.HasValue)
                {
                    table.Errors = 0;
                    table.ErrorsRows = new List<int>();
                }
                using (var reader = new StreamReader(path))
                {                    
                    while (!reader.EndOfStream)
                    {
                        if (counter == 1) { reader.ReadLine(); }
                        if (counter > 1) { AddRow(table, tableNode, researchIndexNode, reader.ReadLine(), counter); }
                        if ((counter % 500) == 0) { _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("{0} rows added", counter) }); }
                        counter++;
                    }
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("{0} rows added", counter - 1) });
                }                
                EndWriter();
            }
            catch (Exception ex)
            {
                result = false;
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("AddFile Failed: {0}", ex.Message) });
            }
            return result;
        }

        private void AddRow(Table table, XElement tableNode, XElement researchIndexNode, string line, int index)
        {
            _writer.WriteStartElement("row");
            var rowError = false;
            var row = line.Split(Separator).ToList();
            if(line.IndexOf("\"") > -1) { row = ParseRow(line); }
            for(int i = 0; i < table.Columns.Count; i++)
            {                
                var column = table.Columns[i];
                var value = row[i];
                string convertedValue = null;
                if (string.IsNullOrEmpty(value.Trim()) && column.Nullable)
                {
                    convertedValue = string.Empty;
                }
                else
                {
                    var hasError = false;
                    HandleSpecialNumeric(column, tableNode, researchIndexNode, value);
                    convertedValue = GetConvertedValue(column, value, out hasError);
                    if (hasError)
                    {
                        rowError = true;
                        table.Errors++;                        
                        _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = string.Format("Convert column {0} of type {1} with value {2} has error", column.Name, column.Type, value) });
                    }
                }
                _writer.WriteElementString(column.Id, convertedValue);
            }
            if (rowError) { table.ErrorsRows.Add(index); }
            _writer.WriteEndElement();
        }

        private void HandleSpecialNumeric(Column column, XElement tableNode, XElement researchIndexNode, string value)
        {
            if(!_regExps.ContainsKey(SpecialNumericPattern))
            {
                _regExps.Add(SpecialNumericPattern, new Regex(SpecialNumericPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            }
            if((column.Type == "INTEGER" || column.Type == "DECIMAL") && _regExps[SpecialNumericPattern].IsMatch(value))
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Handle Special Numeric value {0} at column {1}", value, column.Name) });
                column.Type = string.Format(VarCharPrefix, GetColumnLength(column.Type, column.RegExp));
                column.Modified = true;

                var columnNode = tableNode.Element(_tableIndexXNS + "columns").Elements().Where(e => e.Element(_tableIndexXNS + "columnID").Value == column.Id).FirstOrDefault();
                columnNode.Element(_tableIndexXNS + "type").Value = column.Type;

                researchIndexNode.Element(_tableIndexXNS + "specialNumeric").Value = true.ToString().ToLower();
                AddMissingColumnNode(value, researchIndexNode, column.Id);
               _updateDocuments = true;
            }
        }

        private List<string> ParseRow(string line)
        {
            var result = new List<string>();
            var offset = 0;
            var column = ParseColumn(line, offset);
            result.Add(column);
            offset += (column.Length + 1);
            while (offset < (line.Length - 1))
            {
                column = ParseColumn(line, offset);
                result.Add(column);
                offset += (column.Length + 1);
            }
            return result;
        }

        private string ParseColumn(string line, int offset)
        {
            var startIndex = line.IndexOf("\"", offset);
            var endIndex = -1;
            if (startIndex == offset)
            {
                endIndex = line.IndexOf("\";", offset);
                if (endIndex == -1)
                {
                    endIndex = line.IndexOf(";", offset);
                }
                if (endIndex > -1) { endIndex++; }
            }
            else
            {
                startIndex = offset;
                endIndex = line.IndexOf(";", offset);
            }

            return (endIndex > -1) ? line.Substring(startIndex, endIndex - startIndex) : line.Substring(startIndex);
        }

        private List<string> StreamLine(string fileName, int index)
        {
            List<string> result = null;
            var counter = 0;
            using (var reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream && counter <= index)
                {
                    var line = reader.ReadLine();
                    if (counter == index) {
                        result = line.Split(Separator).ToList();
                        if (line.IndexOf("\"") > -1) { result = ParseRow(line); }
                    }
                    counter++;
                }
            }
            return result;
        }

        private XElement StreamElement(string fileName, int index)
        {
            XElement result = null;
            int counter = 0;
            using (var rdr = XmlReader.Create(fileName))
            {
                rdr.MoveToContent();
                while (rdr.Read() && counter < index)
                {
                    if ((rdr.NodeType == XmlNodeType.Element) && (rdr.Name == "row"))
                    {
                        result = XNode.ReadFrom(rdr) as XElement;
                        counter++;
                    }
                }
                rdr.Close();
            }
            return result;
        }        

        private string GetConvertedValue(Column column, string value, out bool hasError)
        {
            string result = null;
            switch (column.Type)
            {
                case "INTEGER": result = GetIntegerValue(column, value, out hasError);break;
                case "DECIMAL": result = GetDecimalValue(column, value, out hasError); break;
                case "DATE": result = GetDateValue(column, value, out hasError); break;
                case "TIME": result = GetTimeValue(column, value, out hasError); break;
                case "TIMESTAMP": result = GetTimeStampValue(column, value, out hasError); break;
                default: result = GetStringValue(column, value, out hasError); break;                    
            }
            return result;
        }

        private string GetTimeStampValue(Column column, string value, out bool hasError)
        {
            hasError = false;
            var result = value;
            if (!_regExps.ContainsKey(column.RegExp))
            {
                _regExps.Add(column.RegExp, new Regex(column.RegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            }
            hasError = !_regExps[column.RegExp].IsMatch(result);
            if (!hasError)
            {
                var groups = _regExps[column.RegExp].Match(result).Groups;
                if (column.RegExp == "^([0-9]{2,2})-([a-zA-Z]{3,3})-([0-9]{4,4})\\s([0-9]{2,2}):([0-9]{2,2}):([0-9]{2,2})$")
                {
                    result = string.Format("{0}-{1}-{2}T{3}:{4}:{5}", groups[3].Value, GetMonth(groups[2].Value), groups[1].Value, groups[4].Value, groups[5].Value, groups[5].Value);
                }
                else
                {
                    result = string.Format("{0}-{1}-{2}T{3}:{4}:{5}", groups[1].Value, groups[2].Value, groups[3].Value, groups[4].Value, groups[5].Value, groups[5].Value);
                }
                
            }
            return result;
        }

        private string GetMonth(string monthValue)
        {
            string result = null;
            switch (monthValue)
            {
                case "JAN": result = "01"; break;
                case "FEB": result = "02"; break;
                case "MAR": result = "03"; break;
                case "APR": result = "04"; break;
                case "MAY": result = "05"; break;
                case "JUN": result = "06"; break;
                case "JUL": result = "07"; break;
                case "AUG": result = "08"; break;
                case "SEP": result = "09"; break;
                case "OCT": result = "10"; break;
                case "NOV": result = "11"; break;
                case "DEC": result = "12"; break;
            }
            return result;
        }

        private string GetTimeValue(Column column, string value, out bool hasError)
        {
            hasError = false;
            var result = value;
            if (!_regExps.ContainsKey(column.RegExp))
            {
                _regExps.Add(column.RegExp, new Regex(column.RegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            }
            hasError = !_regExps[column.RegExp].IsMatch(result);
            if (!hasError)
            {
                var groups = _regExps[column.RegExp].Match(result).Groups;
                result = string.Format("{0}:{1}:{2}", groups[1].Value, groups[2].Value, groups[3].Value);
            }
            return result;
        }

        private string GetDateValue(Column column, string value, out bool hasError)
        {
            hasError = false;
            var result = value;
            if (!_regExps.ContainsKey(column.RegExp))
            {
                _regExps.Add(column.RegExp, new Regex(column.RegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            }
            hasError = !_regExps[column.RegExp].IsMatch(result);
            if (!hasError)
            {
                var groups = _regExps[column.RegExp].Match(result).Groups;
                result = string.Format("{0}-{1}-{2}", groups[1].Value, groups[2].Value, groups[3].Value);
            }
            return result;
        }

        private string GetStringValue(Column column, string value, out bool hasError)
        {
            hasError = false;
            var result = value;
            if (result.IndexOf("\"") > -1)
            {
                if (!_regExps.ContainsKey(DoubleApostrophePattern))
                {
                    _regExps.Add(DoubleApostrophePattern, new Regex(DoubleApostrophePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
                }
                hasError = !_regExps[DoubleApostrophePattern].IsMatch(result);
                if (!hasError)
                {
                    result = _regExps[DoubleApostrophePattern].Match(result).Groups[1].Value;
                    result = result.Replace("\"\"", "\"");
                }
            }
            return result;
        }

        private string GetIntegerValue(Column column, string value, out bool hasError)
        {
            int result = -1;
            hasError = !int.TryParse(value, out result);
            return result.ToString();
        }

        private string GetDecimalValue(Column column, string value, out bool hasError)
        {
            float result = -1;
            hasError = !float.TryParse(value.Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            return result.ToString(nfi);
        }
    }
}
