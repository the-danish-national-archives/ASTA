using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena.Logging;
using System;
using System.Collections.Generic;
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
        private Regex _specialNumeric = null;
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
           _specialNumeric = new Regex(SpecialNumericPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
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
        /// <param name="preview"></param>
        /// <returns></returns>
        public Row GetRow(Table table,int index, bool preview = false)
        {
            Row result = null;
            var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", table.Folder));
            if (File.Exists(path))
            {
                XNamespace tableNS = string.Format(TableXmlNs, table.Folder);
                XElement rowNode = StreamElement(path, index);
                if(rowNode != null)
                {
                    if (!preview) { result = new Row() { DestValues = new Dictionary<string, string>(), SrcValues = new Dictionary<string, string>(), ErrorsColumns = new List<string>() }; }
                    table.Columns.ForEach(c =>
                    {
                        var hasError = false;
                        var value = rowNode.Element(tableNS + c.Id).Value;
                        var newValue = (string.IsNullOrEmpty(value.Trim()) && c.Nullable) ? value : GetConvertedValue(c.Type, value, out hasError);
                        if (!preview)
                        {
                            result.SrcValues.Add(c.Id, value);
                            result.DestValues.Add(c.Id, newValue);
                        }
                        if (hasError || value != newValue)
                        {
                            if (!preview) { result.ErrorsColumns.Add(c.Id); }
                            table.Errors++;
                        }
                    });
                }
            }
            else
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("None Exists data file: {0}", path) });
            }
            return result;
        }

        private bool AddFile(Table table)
        {
            var result = true;
            try
            {
                var tableNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "folder").Value == table.Folder).FirstOrDefault();
                var counter = 1;
                var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", table.Folder));
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add file: {0} ", path) });
                StartWriter(table.Folder);
                path = string.Format("{0}\\Data\\{1}\\{1}.csv", _srcPath.Substring(0, _srcPath.LastIndexOf(".")), table.SrcFolder);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Loop file: {0} ", path) });
                using (var reader = new StreamReader(path))
                {                    
                    while (!reader.EndOfStream)
                    {
                        if (counter == 1) { reader.ReadLine(); }
                        if (counter > 1) { AddRow(table, tableNode, reader.ReadLine()); }
                        if ((counter % 500) == 0) {
                            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("{0} rows added", counter) });
                        }
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

        private void AddRow(Table table, XElement tableNode, string line)
        {
            _writer.WriteStartElement("row");
            var row = line.Split(Separator).ToList();
            if(line.IndexOf("\"") > -1)
            {
                row = ParseRow(line);
            }
            for(int i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                var value = row[i];
                if(string.IsNullOrEmpty(value.Trim()) && column.Nullable) { value = string.Empty; }
                HandleSpecialNumeric(column, tableNode, value);
                _writer.WriteElementString(column.Id, value);
            }
            _writer.WriteEndElement();
        }

        private void HandleSpecialNumeric(Column column, XElement tableNode,string value)
        {
            if((column.Type == "INTEGER" || column.Type == "DECIMAL") && _specialNumeric.IsMatch(value))
            {
                column.Type = string.Format(VarCharPrefix,50);
                column.Modified = true;
                var columnNode = tableNode.Element(_tableIndexXNS + "columns").Elements().Where(e => e.Element(_tableIndexXNS + "columnID").Value == column.Id).FirstOrDefault();
                columnNode.Element(_tableIndexXNS + "type").Value = column.Type;
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

        private string GetConvertedValue(string type,string value, out bool hasError)
        {
            switch (type)
            {
                case "INTEGER":
                    {
                        int result = -1;
                        hasError = !int.TryParse(value, out result);
                        return result.ToString();
                    }; break;
                case "DECIMAL":
                    {
                        float result = -1;
                        hasError = !float.TryParse(value, out result);
                        return result.ToString();
                    }; break;
                /*case "DATE": result = "DATE"; break;
                case "TIME": result = "TIME"; break;
                case "TIMESTAMP": result = "TIMESTAMP"; break;*/
                default:
                    {
                        hasError = false;
                        return value;
                    }; break;
            }
        }
    }
}
