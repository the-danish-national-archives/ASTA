using Rigsarkiv.Athena;
using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena.Logging;
using System;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;

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
        private Index _converter = null;
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
            _outputRichTextBox.Location = new Point(2, 380);
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
                reportButton.Enabled = true;
            }
        }

        private void Render()
        {
            tablesDataGridView.ClearSelection();
            AddRow(tablesDataGridView, 0, TablesCounter, _report.Tables.Count, _report.TablesCounter);
            AddRow(tablesDataGridView, 1, codeListsCounter, _report.Tables.Select(t => t.CodeList.Count).Sum(), _report.CodeListsCounter);
            
            var rowIndex = 0;
            var columnIndex = 0;
            rowsDataGridView.ClearSelection();
            _report.Tables.ForEach(mainTable => {
                AddRow(rowsDataGridView, rowIndex++, string.Format("{0} ({1})", mainTable.Folder, mainTable.Name), mainTable.Rows, mainTable.RowsCounter);
                if(mainTable.Errors > 0)
                {
                    mainTable.Columns.ForEach(c =>  {
                        if(c.Errors > 0) {
                            AddColumn(columnsDataGridView, columnIndex++, string.Format("{0} ({1})", mainTable.Folder, mainTable.Name), c.Name,c.Type,c.Errors);
                        }
                    });
                }
                mainTable.CodeList.ForEach(table => {
                    AddRow(rowsDataGridView, rowIndex++, string.Format("{0} ({1})", table.Folder, table.Name), table.Rows, table.RowsCounter);
                    if (table.Errors > 0)
                    {
                        table.Columns.ForEach(c => {
                            if (c.Errors > 0)
                            {
                                AddColumn(columnsDataGridView, columnIndex++, string.Format("{0} ({1})", mainTable.Folder, mainTable.Name), c.Name, c.Type, c.Errors);
                            }
                        });
                    }
                });
            });
        }

        private void AddRow(DataGridView view,int index,string title,int before, int after)
        {
            view.Rows.Add(1);
            view[0, index].Value = title;
            view[1, index].Value = before;
            view[2, index].Value = after;
            if (before != after) {
                view[1, index].Style.BackColor = Color.Red;
                view[2, index].Style.BackColor = Color.Red;
            }
        }

        private void AddColumn(DataGridView view, int index, string title, string variable,string dataType, int errors)
        {
            view.Rows.Add(1);
            view[0, index].Value = title;
            view[1, index].Value = variable;
            view[2, index].Value = dataType;
            view[3, index].Value = errors;
            if (errors > 0)
            {
                view[3, index].Style.BackColor = Color.Red;
            }
        }

        private void logButton_Click(object sender, System.EventArgs e)
        {
            var path = string.Format("{0}\\{1}_log.html", _destPath, _destFolder);
            if (_logManager.Flush(path))
            {
                OpenFile(path);
            }
        }

        private void Form3_Shown(object sender, EventArgs e)
        {
            _log.Info("Run");
        }

        private void reportButton_Click(object sender, EventArgs e)
        {
            var path = string.Format("{0}\\{1}_report.html", _destPath, _destFolder);
            if (_converter.Flush(path))
            {
                OpenFile(path);
            }
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
