using Rigsarkiv.Athena.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena;

namespace Rigsarkiv.AthenaForm
{
    public partial class Form2 : Form
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Form2));
        const string RowsLabel = "Række {0} ud af {1}";
        const string TableErrorsLabel = "Fejl i tabel {0}";
        const string RowErrorsLabel = "Fejl i række: {0}";
        const string CodeTableLabel = "Kodetabel: {0}";
        const string MainTableLabel = "Hovedtabel: {0}";
        private LogManager _logManager = null;
        private string _srcPath = null;
        private string _destPath = null;
        private string _destFolder = null;
        private Data _converter = null;
        private Report _report = null;
        private Table _mainTable = null;
        private Table _codeTable = null;
        private RichTextBox _outputRichTextBox = null;
        private int _rowIndex = 1;
        private int _selectedColumn = -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        /// <param name="logManager"></param>
        /// <param name="tables"></param>
        /// <param name="outputRichTextBox"></param>
        public Form2(string srcPath, string destPath,string destFolder, LogManager logManager, Report report, RichTextBox outputRichTextBox)
        {
            InitializeComponent();            
            _logManager = logManager;
            _srcPath = srcPath;
            _destPath = destPath;
            _destFolder = destFolder;
             _converter = new Data(logManager, srcPath, destPath, destFolder, report);
            _report = report;
            _outputRichTextBox = outputRichTextBox;
            mainTablesListBox.Items.AddRange(_report.Tables.Select(t => t.Name).ToArray());
            rowLabel.Text = "";
            tableInfoLabel.Text = "";
            rowErrorsLabel.Text = "";
            tableErrorsLabel.Text = "";
            Text = string.Format(Text, destFolder);
        }

        public void tablesBox_Click(object sender, EventArgs e)
        {
           
        }

        private void mainTablesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {

        }

        private void codeTablesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            this.SuspendLayout();
            e.DrawBackground();

            Graphics g = e.Graphics;
            g.FillRectangle(new SolidBrush(Color.White), e.Bounds);
            ListBox lb = (ListBox)sender;
            g.DrawString(lb.Items[e.Index].ToString(), e.Font, new SolidBrush(Color.Red), new PointF(e.Bounds.X, e.Bounds.Y));

            e.DrawFocusRectangle();
            this.ResumeLayout();
        }

        private void codeTablesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(codeTablesListBox.SelectedIndex == -1) { return; }
            _selectedColumn = -1;
            nextErrorButton.Enabled = false;
            _rowIndex = 1;
            rowLabel.Text = "";
            rowErrorsLabel.Text = "";
            tableErrorsLabel.Text = "";
            dataValues.Rows.Clear();            
            _codeTable = _mainTable.CodeList[codeTablesListBox.SelectedIndex];
            tableInfoLabel.Text = string.Format(CodeTableLabel, _codeTable.Name);            
            UpdateDataRow(_codeTable);
            nextButton.Enabled = prevButton.Enabled = searchButton.Enabled = true;            
        }

        private void mainTablesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedColumn = -1;
            nextErrorButton.Enabled = false;
            _rowIndex = 1;
            rowLabel.Text = "";
            rowErrorsLabel.Text = "";
            tableErrorsLabel.Text = "";
            nextButton.Enabled = prevButton.Enabled = searchButton.Enabled = false;
            dataValues.Rows.Clear();
           _codeTable = null;
            codeTablesListBox.Items.Clear();
            _mainTable = _report.Tables[mainTablesListBox.SelectedIndex];
            if(_mainTable.CodeList != null && _mainTable.CodeList.Count > 0)
            {
                codeTablesListBox.Items.AddRange(_mainTable.CodeList.Select(t => t.Name).ToArray());
            }
            tableInfoLabel.Text = string.Format(MainTableLabel, _mainTable.Name);            
            UpdateDataRow(_mainTable);
            nextButton.Enabled = prevButton.Enabled = searchButton.Enabled = true;            
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            _rowIndex++;
            var table = (_codeTable != null) ? _codeTable : _mainTable;
            if (table.Rows >= _rowIndex)
            {
                UpdateDataRow(table);
            }
            searchTextBox.Text = _rowIndex.ToString();
            nextButton.Enabled = (table.Rows > _rowIndex);
            prevButton.Enabled = true;
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            _rowIndex--;
            var table = (_codeTable != null) ? _codeTable : _mainTable;
            if (_rowIndex >= 1 && _rowIndex <= table.Rows)
            {
               UpdateDataRow(table);
            }
            searchTextBox.Text = _rowIndex.ToString();
            prevButton.Enabled = (_rowIndex > 1);            
            nextButton.Enabled = true;
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            if(int.TryParse(searchTextBox.Text,out _rowIndex))
            {
                var table = (_codeTable != null) ? _codeTable : _mainTable;
                if (_rowIndex > 0 && _rowIndex <= table.Rows)
                {
                    UpdateDataRow(table);
                    prevButton.Enabled = (_rowIndex > 1);
                    nextButton.Enabled = (table.Rows > _rowIndex);
                }
            }
        }
        private void IndexButton_Click(object sender, EventArgs e)
        {
            Form3 form = new Form3(_srcPath, _destPath, _destFolder, _logManager, _converter.Report, _outputRichTextBox);
            form.Show();
            this.Hide();
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            _log.Info("Run");
        }

        private void dataValues_SelectionChanged(object sender, EventArgs e)
        {
                       
        }

        private void nextErrorButton_Click(object sender, EventArgs e)
        {
            var table = (_codeTable != null) ? _codeTable : _mainTable;
            var errorRows = table.Columns[_selectedColumn].ErrorsRows;
            if (errorRows.Any(index => (_rowIndex - 1) < index))
            {
                var nextIndex = errorRows.FirstOrDefault(index => (_rowIndex - 1) < index);
                _rowIndex = nextIndex + 1;
                searchTextBox.Text = _rowIndex.ToString();
                UpdateDataRow(table);
                prevButton.Enabled = (_rowIndex > 1);
                nextButton.Enabled = (table.Rows > _rowIndex);
            }
        }

        private void dataValues_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var table = (_codeTable != null) ? _codeTable : _mainTable;
            if(e.ColumnIndex == 0)
            {
                if (e.RowIndex == _selectedColumn)
                {
                    dataValues.Rows[_selectedColumn].DefaultCellStyle.Font = new Font(dataValues.Font, FontStyle.Regular);
                    dataValues.ClearSelection();
                    _selectedColumn = -1;
                    nextErrorButton.Enabled = false;
                }
                else
                {
                    if (_selectedColumn > -1)
                    {
                        dataValues.Rows[_selectedColumn].DefaultCellStyle.Font = new Font(dataValues.Font, FontStyle.Regular);
                    }
                    _selectedColumn = e.RowIndex;                    
                    UpdateErrorButton(table);
                }
            }
            valueRichTextBox.Text = string.Empty;
            if (e.ColumnIndex == 2 || e.ColumnIndex == 4)
            {
                valueRichTextBox.Text = dataValues[e.ColumnIndex, e.RowIndex].Value.ToString();
            }
        }

        private void UpdateDataRow(Table table)
        {
            valueRichTextBox.Text = string.Empty;
            rowLabel.Text = string.Format(RowsLabel, _rowIndex, table.Rows);
            dataValues.Rows.Clear();            
            var row = _converter.GetRow(table, _rowIndex);
            dataValues.Rows.Add(table.Columns.Count);
            for (int i = 0; i < table.Columns.Count; i++)
            {                
                var column = table.Columns[i];
                dataValues[0, i].Value = column.Name;
                dataValues[1, i].Value = column.TypeOriginal;
                if(column.Modified) { dataValues[1, i].Style.BackColor = Color.LightGreen; }
                if (row != null && row.SrcValues.ContainsKey(column.Id))
                {
                    dataValues[2, i].Value = row.SrcValues[column.Id];
                    if (row.SrcValues[column.Id] != row.DestValues[column.Id]) { dataValues[2, i].Style.BackColor = Color.LightGreen; }
                    if (row.ErrorsColumns.Contains(column.Id)) { dataValues[2, i].Style.BackColor = Color.Red; }                    
                }
                dataValues[3, i].Value = column.Type;
                if (column.Modified) { dataValues[3, i].Style.BackColor = Color.LightGreen; }
                if (row != null && row.SrcValues.ContainsKey(column.Id))
                {
                    dataValues[4, i].Value = row.DestValues[column.Id];
                    if (row.SrcValues[column.Id] != row.DestValues[column.Id]) { dataValues[4, i].Style.BackColor = Color.LightGreen; }
                    if (row.ErrorsColumns.Contains(column.Id)) { dataValues[4, i].Style.BackColor = Color.Red; }                    
                }
                dataValues[5, i].Value = column.Differences;
                dataValues[6, i].Value = column.ErrorsRows.Count > 0 ? column.ErrorsRows.Count : 0;
            }
            tableErrorsLabel.Text = string.Format(TableErrorsLabel, table.Errors);
            if (row != null) { rowErrorsLabel.Text = string.Format(RowErrorsLabel, row.ErrorsColumns.Count); }
            dataValues.ClearSelection();
            if (_selectedColumn > -1)
            {
                dataValues.Rows[_selectedColumn].Cells[0].Selected = true;
                dataValues.FirstDisplayedScrollingRowIndex = _selectedColumn;
                UpdateErrorButton(table);
            }
        }

        private void UpdateErrorButton(Table table)
        {
           var errorRows = table.Columns[_selectedColumn].ErrorsRows;
           nextErrorButton.Enabled = errorRows.Any(index => (_rowIndex - 1) < index);
           dataValues.Rows[_selectedColumn].DefaultCellStyle.Font = new Font(dataValues.Font, FontStyle.Bold);
        }
    }
}
