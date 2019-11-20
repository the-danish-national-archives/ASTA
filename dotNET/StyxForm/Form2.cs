using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Styx;
using Rigsarkiv.Styx.Entities;
using Rigsarkiv.StyxForm.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rigsarkiv.StyxForm
{
    public partial class Form2 : Form
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Form2));
        const string CodeTableLabel = "Kodetabel: {0}";
        const string MainTableLabel = "Hovedtabel: {0}";
        const string LogPath = "{0}\\{1}_ASTA_konverteringslog.html";
        const string ReportPath = "{0}\\{1}_ASTA_konverteringsrapport.html";
        private LogManager _logManager = null;
        private Converter _converter = null;
        private string _srcPath = null;
        private string _destPath = null;
        private string _destFolder = null;
        private Report _report = null;
        private RichTextBox _outputRichTextBox = null;
        private Table _mainTable = null;
        private Table _codeTable = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        /// <param name="logManager"></param>
        /// <param name="report"></param>
        public Form2(string srcPath, string destPath, string destFolder, LogManager logManager,Report report)
        {
            InitializeComponent();
            _logManager = logManager;
            _srcPath = srcPath;
            _destPath = destPath;
            _destFolder = destFolder;
            _report = report;
            
            mainTablesListBox.Items.AddRange(_report.Tables.Select(t => t.Title).ToArray());
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (codeTablesListBox.SelectedIndex == -1) { return; }
            _report.Tables.Add(_codeTable);
            var column = _mainTable.Columns.Where(c => c.CodeList != null).ToList()[codeTablesListBox.SelectedIndex];
            column.CodeList = null;
            codeTablesListBox.Items.RemoveAt(codeTablesListBox.SelectedIndex);
            mainTablesListBox.Items.Add(_codeTable.Title);
            _codeTable = null;            
            codeTablesListBox.ClearSelected();
        }

        private void mainTablesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mainTablesListBox.SelectedIndex == -1) { return; }
            _codeTable = null;
            codeTablesListBox.Items.Clear();
            _mainTable = _report.Tables[mainTablesListBox.SelectedIndex];            
            if (_mainTable.Columns != null && _mainTable.Columns.Count > 0 && _mainTable.Columns.Any(c => c.CodeList != null))
            {
                codeTablesListBox.Items.AddRange(_mainTable.Columns.Where(c => c.CodeList != null).Select(t => t.CodeList.Title).ToArray());
            }
            tableInfoLabel.Text = string.Format(MainTableLabel, _mainTable.Title);
        }

        private void codeTablesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (codeTablesListBox.SelectedIndex == -1) { return; }
            removeButton.Enabled = true;
            _codeTable = _mainTable.Columns.Where(c => c.CodeList != null).Select(t => t.CodeList).ToList()[codeTablesListBox.SelectedIndex];
            tableInfoLabel.Text = string.Format(CodeTableLabel, _codeTable.Title);
        }

        private void convertButton_Click(object sender, EventArgs e)
        {
            reportButton.Enabled = false;
            scriptLabel1.Visible = false;
            scriptLabel2.Visible = false;
            scriptLabel3.Visible = false;
            Cursor.Current = Cursors.WaitCursor;
            outputRichTextBox.Clear();
            _logManager = new LogManager();
            _logManager.LogAdded += OnLogAdded;
            _converter = new Structure(_logManager, _srcPath, _destPath, _destFolder, _report, FlowState.Completed);
            if (_converter.Run())
            {
                var tableIndexXDocument = _converter.TableIndexXDocument;
                var researchIndexXDocument = _converter.ResearchIndexXDocument;
                _converter = new MetaData(_logManager, _srcPath, _destPath, _destFolder, _converter.Report, _converter.State) { TableIndexXDocument = tableIndexXDocument, ResearchIndexXDocument = researchIndexXDocument };
                if (_converter.Run() && (_converter.State == FlowState.Running || _converter.State == FlowState.Completed))
                {
                    _converter = new Data(_logManager, _srcPath, _destPath, _destFolder, _converter.Report, _converter.State);
                    if (_converter.Run() && ((Data)_converter).Flush(string.Format(ReportPath, _destPath, _destFolder), _destFolder))
                    {
                        reportButton.Enabled = true;
                        scriptLabel1.Text = string.Format(scriptLabel1.Text, _destFolder);
                        scriptLabel1.Visible = true;
                        scriptLabel2.Visible = true;
                        scriptLabel3.Visible = true;
                    }
                }
            }
            var path = string.Format(LogPath, _destPath, _destFolder);
            if (_logManager.Flush(path, _destFolder, _converter.GetLogTemplate())) { logButton.Enabled = true; }
            Cursor.Current = Cursors.Default;
        }

        private void OnLogAdded(object sender, LogEventArgs e)
        {
            var message = e.LogEntity.Message;
            switch (e.LogEntity.Level)
            {
                case LogLevel.Error: outputRichTextBox.AppendText(message, Color.Red); break;
                case LogLevel.Info: outputRichTextBox.AppendText(message, Color.Black); break;
                case LogLevel.Warning: outputRichTextBox.AppendText(message, Color.Orange); break;
            }
            //System.Threading.Thread.Sleep(200);
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            _log.Info("Run");
        }

        private void reportButton_Click(object sender, EventArgs e)
        {
            var path = string.Format(ReportPath, _destPath, _destFolder);
            Cursor.Current = Cursors.WaitCursor;
            OpenFile(path);
            Cursor.Current = Cursors.Default;
        }

        private void logButton_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var path = string.Format(LogPath, _destPath, _destFolder);
            OpenFile(path);
            Cursor.Current = Cursors.Default;
        }

        private bool OpenFile(string path)
        {
            var result = true;
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error(string.Format("Start Process {0} Failed", path), ex);
            }
            return result;
        }
    }
}
