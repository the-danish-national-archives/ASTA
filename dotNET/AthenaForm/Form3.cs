using Rigsarkiv.Athena;
using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena.Logging;
using System;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rigsarkiv.AthenaForm
{
    public partial class Form3 : Form
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Form3));
        const string TablesCounter = "Antal tabeller";
        const string codeListsCounter = "Antal kodetabeller";
        private LogManager _logManager = null;
        private string _srcPath = null;
        private string _destPath = null;
        private string _destFolder = null;
        private Converter _converter = null;
        private Report _report = null;
        private RichTextBox _outputRichTextBox = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        /// <param name="logManager"></param>
        /// <param name="tables"></param>
        public Form3(string srcPath, string destPath, string destFolder, LogManager logManager, Report report, RichTextBox outputRichTextBox)
        {
            InitializeComponent();
            _logManager = logManager;
            _srcPath = srcPath;
            _destPath = destPath;
            _destFolder = destFolder;
            _report = report;
            _outputRichTextBox = outputRichTextBox;
            _outputRichTextBox.Location = new Point(2, 360);
            this.Controls.Add(_outputRichTextBox);
            var result = Convert();
            Text = string.Format(Text, destFolder);  
            Render();
        }

        private async Task Convert()
        {
            await Task.Delay(500);
            _converter = new Index(_logManager, _srcPath, _destPath, _destFolder,_report);
            if(_converter.Run())
            {
                logButton.Enabled = true;                
            }
        }

        private void Render()
        {
            tablesDataGridView.ClearSelection();
            tablesDataGridView.Rows.Add(2);
            tablesDataGridView[0, 0].Value = TablesCounter;
            tablesDataGridView[1, 0].Value = _report.Tables.Count;
            tablesDataGridView[2, 0].Value = _report.TablesCounter;
            tablesDataGridView[0, 1].Value = codeListsCounter;
            tablesDataGridView[1, 1].Value = _report.Tables.Select(t => t.CodeList.Count).Sum();
            tablesDataGridView[2, 1].Value = _report.CodeListsCounter;
        }

        private void logButton_Click(object sender, System.EventArgs e)
        {
            var path = string.Format("{0}\\{1}.html", _destPath, _destFolder);
            if (_logManager.Flush(path))
            {
                try
                {
                    Process.Start(path);
                }
                catch (Exception ex)
                {
                    _log.Error(string.Format("Start Process {0} Failed", path), ex);
                }
                
            }
        }

        private void Form3_Shown(object sender, EventArgs e)
        {
            _log.Info("Run");
        }
    }
}
