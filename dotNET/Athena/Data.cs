using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena.Logging;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// Data converter
    /// https://lennilobel.wordpress.com/2009/09/02/streaming-into-linq-to-xml-using-c-custom-iterators-and-xmlreader/
    /// </summary>
    public class Data : Converter
    {
        const int RowsChunk = 500;
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
                    t.CodeList.ForEach(cl =>
                    {
                        if (!AddCodes(cl)) { result = false; }
                    });
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

        private bool AddCodes(Table codeList)
        {
            var result = true;
            try
            {
                var codeListColumn = codeList.Columns[0];
                var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", codeList.Folder));
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Add file: {0} ", path) });
                StartWriter(codeList.Folder);
                foreach (var pair in codeList.Options)
                {
                    var hasError = false;
                    var isDifferent = false;
                    _writer.WriteStartElement("row");
                    _writer.WriteElementString("c1", GetConvertedValue(codeListColumn, pair.Key, out hasError, out isDifferent));
                    _writer.WriteElementString("c2", pair.Value);
                    _writer.WriteEndElement();
                    if (isDifferent) { codeListColumn.Differences++; }
                    if (hasError)
                    {
                        codeListColumn.Errors++;
                        if (MaxErrorsRows > codeListColumn.ErrorsRows.Count)
                        {
                            codeListColumn.ErrorsRows.Add(codeList.Errors);
                            _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = string.Format("Convert column {0} of type {1} with value {2} has error", codeListColumn.Name, codeListColumn.Type, pair.Key) });
                        }
                        codeList.Errors++;
                    }
                    codeList.RowsCounter++;
                }
                EndWriter();
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Column {0} of table {1} has {2} Differences", codeListColumn.Name, codeList.Folder, codeListColumn.Differences) });
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("AddCodes Failed", ex);
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("AddCodes Failed: {0}", ex.Message) });
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
                        if ((counter % RowsChunk) == 0) { _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("{0} of {1} rows added", counter,table.Rows) }); }
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
    }
}
