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
        private bool _updateDocuments = false;

        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        /// <param name="report"></param>
        public Data(LogManager logManager, string srcPath, string destPath, string destFolder, Report report) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Data";
            _report = report;            
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
            if (_report.Tables != null && _report.Tables.Count > 0)
            {               
                result = EnsureData();
                if(result) { result = UpdateReport(); }
            }
            else
            {
                message = "Tables metadata Property is empty";
                _log.Info(message);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = message });
            }
            message = result ? "End Converting Data" : "End Converting Data with errors";
            _log.Info(message);
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
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
                if (rowNode != null)
                {
                    var counter = 0;
                     result = new Row() { DestValues = new Dictionary<string, string>(), SrcValues = new Dictionary<string, string>(), ErrorsColumns = new List<string>() };
                    if (rowLine != null)
                    {
                        table.Columns.ForEach(column =>
                        {
                            UpdateRow(table, result, column, rowLine[counter], rowNode.Element(tableNS + column.Id).Value, index - 1);
                            counter++;
                        });
                    }
                    else
                    {
                        var column = table.Columns[0];
                        UpdateRow(table, result, column, table.Options[index - 1], rowNode.Element(tableNS + column.Id).Value, index - 1);
                        column = table.Columns[1];
                        UpdateRow(table, result, column, rowNode.Element(tableNS + column.Id).Value, rowNode.Element(tableNS + column.Id).Value, index - 1);
                    }
                }
            }
            else
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("None Exist file: {0}", xmlPath) });
            }
            return result;
        }
        
        private void UpdateRow(Table table,Row row,Column column, string value, string newValue, int index)
        {
            var hasError = false;
            var isDifferent = false;
            if (string.IsNullOrEmpty(value.Trim()) && column.Nullable) { value = string.Empty; }
            if (column.ErrorsRows.Contains(index)) { GetConvertedValue(column, value, out hasError, out isDifferent); }
            row.SrcValues.Add(column.Id, value);
            row.DestValues.Add(column.Id, newValue);
            if (hasError) { row.ErrorsColumns.Add(column.Id); }
        }

        private bool UpdateReport()
        {
            var result = true;
            try
            {
                var path = string.Format(TablesPath, _destFolderPath);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Count Tables : {0}", path) });
                var files = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories);
                if (files != null && files.Length > 0)
                {
                    files.ToList().ForEach(filePath => {
                        var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
                        fileName = fileName.Substring(0, fileName.LastIndexOf("."));
                        _report.Tables.ForEach(mainTable =>
                        {
                            if (mainTable.Folder == fileName) { _report.TablesCounter++; }
                            if (mainTable.CodeList.Any(t => t.Folder == fileName)) { _report.CodeListsCounter++; }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("UpdateReport Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("UpdateReport Failed: {0}", ex.Message) });
            }
            return result;
        }

        private bool EnsureData()
        {
            var result = true;
            try
            {
                _report.Tables.ForEach(t => {
                    if (!AddFile(t)) { result = false; }
                });
                if (result && _updateDocuments)
                {
                    var path = string.Format(IndicesPath, _destFolderPath);
                    _tableIndexXDocument.Save(string.Format("{0}\\{1}", path, TableIndex));
                    _researchIndexXDocument.Save(string.Format("{0}\\{1}", path, ResearchIndex));
                }
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("EnsureData Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureData Failed: {0}", ex.Message) });
            }
            return result;
        }

        private bool AddFile(Table table)
        {
            var result = true;
            try
            {
                var researchIndexNode = _researchIndexXDocument.Element(_tableIndexXNS + "researchIndex").Element(_tableIndexXNS + "mainTables").Elements().Where(e => e.Element(_tableIndexXNS + "tableID").Value == table.Folder).FirstOrDefault();
                var tableNode = _tableIndexXDocument.Element(_tableIndexXNS + "siardDiark").Element(_tableIndexXNS + "tables").Elements().Where(e => e.Element(_tableIndexXNS + "folder").Value == table.Folder).FirstOrDefault();
                var counter = 0;
                var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", table.Folder));
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add file: {0} ", path) });
                StartWriter(table.Folder);
                path = string.Format("{0}\\Data\\{1}\\{1}.csv", _srcPath.Substring(0, _srcPath.LastIndexOf(".")), table.SrcFolder);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Loop file: {0} ", path) });
                using (var reader = new StreamReader(path))
                {                    
                    while (!reader.EndOfStream)
                    {
                        counter++;
                        if (counter == 1) { reader.ReadLine(); }
                        if (counter > 1) { AddRow(table, tableNode, researchIndexNode, reader.ReadLine(), counter); }
                        if ((counter % 500) == 0) { _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("{0} rows added", counter) }); }
                    }
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("{0} rows added", counter - 1) });
                }                
                EndWriter();
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Table {0} has {1} total Differences", table.Folder, table.Columns.Sum(c => c.Differences)) });
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("AddFile Failed", ex);
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
                    var isDifferent = false;
                    if (HasSpecialNumeric(column, value))
                    {
                        HandleSpecialNumeric(column, tableNode, researchIndexNode, value, true);
                        _updateDocuments = true;
                    }
                    convertedValue = GetConvertedValue(column, value, out hasError,out isDifferent);
                    if (isDifferent) { column.Differences++; }
                    if (hasError)
                    {
                        rowError = true;
                        column.Errors++;
                        if (MaxErrorsRows > column.ErrorsRows.Count)
                        {
                            column.ErrorsRows.Add(index - 2);
                            _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = string.Format("Convert column {0} of type {1} with value {2} has error", column.Name, column.Type, value) });
                        }
                    }
                }
                _writer.WriteElementString(column.Id, convertedValue);
            }
            if (rowError) { table.Errors++; }
            table.RowsCounter++;
            _writer.WriteEndElement();
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
    }
}
