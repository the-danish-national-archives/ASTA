using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Styx.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        private LogManager _logManager = null;
        private string _srcPath = null;
        private string _destPath = null;
        private string _destFolder = null;
        private Report _report = null;
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
            mainTablesListBox.Items.AddRange(_report.Tables.Select(t => t.Name).ToArray());
        }

        private void removeButton_Click(object sender, EventArgs e)
        {

        }

        private void mainTablesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mainTablesListBox.SelectedIndex == -1) { return; }
            _codeTable = null;
            codeTablesListBox.Items.Clear();
            _mainTable = _report.Tables[mainTablesListBox.SelectedIndex];            
            if (_mainTable.Columns != null && _mainTable.Columns.Count > 0 && _mainTable.Columns.Any(c => c.CodeList != null))
            {
                codeTablesListBox.Items.AddRange(_mainTable.Columns.Where(c => c.CodeList != null).Select(t => t.CodeList.Name).ToArray());
            }
            tableInfoLabel.Text = string.Format(MainTableLabel, _mainTable.Name);
        }

        private void codeTablesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (codeTablesListBox.SelectedIndex == -1) { return; }
            removeButton.Enabled = true;
            _codeTable = _mainTable.Columns.Where(c => c.CodeList != null).Select(t => t.CodeList).ToList()[codeTablesListBox.SelectedIndex];
            tableInfoLabel.Text = string.Format(CodeTableLabel, _codeTable.Name);
        }
    }
}
