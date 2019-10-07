using Rigsarkiv.Asta.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena;
using System.Reflection;

namespace Rigsarkiv.AthenaForm
{
    public partial class Form2 : Form
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Form2));
        const string RowsLabel = "Række {0} ud af {1}";
        const string TableErrorsLabel = "Forskelle i tabel {0}";
        const string RowErrorsLabel = "Forskelle i række: {0}";
        const string CodeTableLabel = "Kodetabel: {0}";
        const string MainTableLabel = "Hovedtabel: {0}";
        const string CodeValueHeaderText = "Kode {0} (SIP)";
        const string CodeListValueHeaderText = "Kode {0} (AIP)";
        const string CodeListDescriptionHeaderText = "Kodeforklaring {0}";
        const string C1 = "c1";
        const string C2 = "c2";
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
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dataValues, new object[] { true });
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, codeListValues, new object[] { true });
            _logManager = logManager;
            _srcPath = srcPath;
            _destPath = destPath;
            _destFolder = destFolder;
             _converter = new Data(logManager, srcPath, destPath, destFolder, report);
            _report = report;
            _outputRichTextBox = outputRichTextBox;
            mainTablesListBox.Items.AddRange(_report.Tables.Select(t => t.Name).ToArray());
            Reset(false, false);
            titlelabel.Text = string.Format(titlelabel.Text, destFolder);
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
            infoLabel.Visible = false;
            nextErrorButton.Enabled = prevErrorButton.Enabled = false;
            Reset(false, true);

            _codeTable = _mainTable.CodeList[codeTablesListBox.SelectedIndex];
            tableInfoLabel.Text = string.Format(CodeTableLabel, _codeTable.Name);
            UpdateCodeRow();
            nextButton.Enabled = prevButton.Enabled = searchButton.Enabled = false;
            searchTextBox.Text = _rowIndex.ToString();
        }

        private void mainTablesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {            
            if(mainTablesListBox.SelectedIndex == -1) { return; }
            infoLabel.Visible = false;
            nextErrorButton.Enabled = prevErrorButton.Enabled = false;            
            nextButton.Enabled = prevButton.Enabled = searchButton.Enabled = false;
            Reset(true, false);
            _codeTable = null;
            codeTablesListBox.Items.Clear();
            _mainTable = _report.Tables[mainTablesListBox.SelectedIndex];
            if (_mainTable.CodeList != null && _mainTable.CodeList.Count > 0)
            {
                codeTablesListBox.Items.AddRange(_mainTable.CodeList.Select(t => t.Name.Substring(_mainTable.Name.Length + 1)).ToArray());
            }
            tableInfoLabel.Text = string.Format(MainTableLabel, _mainTable.Name);            
            UpdateDataRow();
            nextButton.Enabled = prevButton.Enabled = searchButton.Enabled = true;
            searchTextBox.Text = _rowIndex.ToString();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _rowIndex++;
            if (_mainTable.Rows >= _rowIndex)
            {
                UpdateDataRow();
            }
            searchTextBox.Text = _rowIndex.ToString();
            nextButton.Enabled = (_mainTable.Rows > _rowIndex);
            prevButton.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _rowIndex--;
            if (_rowIndex >= 1 && _rowIndex <= _mainTable.Rows)
            {
               UpdateDataRow();
            }
            searchTextBox.Text = _rowIndex.ToString();
            prevButton.Enabled = (_rowIndex > 1);            
            nextButton.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            if(int.TryParse(searchTextBox.Text,out _rowIndex))
            {
                Cursor.Current = Cursors.WaitCursor;
                if (_rowIndex > 0 && _rowIndex <= _mainTable.Rows)
                {
                    UpdateDataRow();
                    prevButton.Enabled = (_rowIndex > 1);
                    nextButton.Enabled = (_mainTable.Rows > _rowIndex);
                }
                Cursor.Current = Cursors.Default;
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
            Cursor.Current = Cursors.WaitCursor;
            var errorRows = _mainTable.Columns[_selectedColumn].ErrorsRows;
            if (errorRows.Any(index => (_rowIndex - 1) < index))
            {
                var nextIndex = errorRows.FirstOrDefault(index => (_rowIndex - 1) < index);
                _rowIndex = nextIndex + 1;
                searchTextBox.Text = _rowIndex.ToString();
                UpdateDataRow();
                prevButton.Enabled = (_rowIndex > 1);
                nextButton.Enabled = (_mainTable.Rows > _rowIndex);
            }
            Cursor.Current = Cursors.Default;
        }

        private void prevErrorButton_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var errorRows = _mainTable.Columns[_selectedColumn].ErrorsRows;            
            if (errorRows.Any(index => (_rowIndex - 1) > index))
            {
                var nextIndex = errorRows.LastOrDefault(index => (_rowIndex - 1) > index);
                _rowIndex = nextIndex + 1;
                searchTextBox.Text = _rowIndex.ToString();
                UpdateDataRow();
                prevButton.Enabled = (_rowIndex > 1);
                nextButton.Enabled = (_mainTable.Rows > _rowIndex);
            }
            Cursor.Current = Cursors.Default;
        }

        private void dataValues_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex == -1) { return; }
            if(e.ColumnIndex == 0)
            {
                if (e.RowIndex == _selectedColumn)
                {
                    dataValues.Rows[_selectedColumn].DefaultCellStyle.Font = new Font(dataValues.Font, FontStyle.Regular);
                    dataValues.ClearSelection();
                    _selectedColumn = -1;
                    nextErrorButton.Enabled = false;
                    prevErrorButton.Enabled = false;
                }
                else
                {
                    if (_selectedColumn > -1)
                    {
                        dataValues.Rows[_selectedColumn].DefaultCellStyle.Font = new Font(dataValues.Font, FontStyle.Regular);
                    }
                    _selectedColumn = e.RowIndex;                    
                    UpdateErrorButton();
                }
            }
            valueRichTextBox.Text = string.Empty;
            if (e.ColumnIndex == 1 || e.ColumnIndex == 3 || e.ColumnIndex == 5)
            {
                valueRichTextBox.Text = dataValues[e.ColumnIndex, e.RowIndex].Value.ToString();
            }
        }

        private void codeListValues_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) { return; }
            if (e.ColumnIndex == 0)
            {
                if (e.RowIndex == _selectedColumn)
                {
                    codeListValues.Rows[_selectedColumn].DefaultCellStyle.Font = new Font(dataValues.Font, FontStyle.Regular);
                    codeListValues.ClearSelection();
                    _selectedColumn = -1;
                }
                else
                {
                    if (_selectedColumn > -1)
                    {
                        codeListValues.Rows[_selectedColumn].DefaultCellStyle.Font = new Font(dataValues.Font, FontStyle.Regular);
                    }
                    _selectedColumn = e.RowIndex;
                }
            }
            valueRichTextBox.Text = string.Empty;
            if (e.ColumnIndex == 0 || e.ColumnIndex == 1 || e.ColumnIndex == 2 || e.ColumnIndex == 3)
            {
                valueRichTextBox.Text = codeListValues[e.ColumnIndex, e.RowIndex].Value.ToString();
            }
        }

        private void Reset(bool mainTable, bool codelist)
        {
            _rowIndex = 1;
            rowLabel.Text = "";
            rowErrorsLabel.Text = "";
            tableErrorsLabel.Text = "";
            _selectedColumn = -1;
            if (mainTable)
            {
                dataValues.Rows.Clear();
            }
            if (codelist)
            {
                codeListValues.Rows.Clear();
            }
            dataValues.Visible = mainTable;
            codeListValues.Visible = codelist;
        }

        private void UpdateCodeRow()
        {
            var column = _mainTable.Columns.FirstOrDefault(c => c.CodeListName == _codeTable.Name);
            codeListValues.Columns[0].HeaderText = string.Format(CodeValueHeaderText, column.TypeOriginal);
            codeListValues.Columns[2].HeaderText = string.Format(CodeListValueHeaderText, column.Type);
            codeListValues.Columns[3].HeaderText = string.Format(CodeListDescriptionHeaderText, _codeTable.Columns[1].Type);
            column = _codeTable.Columns[1];
            
            codeListValues.Columns[0].HeaderCell.Style.BackColor = _codeTable.Columns[0].Modified ? Color.LightGreen : Color.White;
            codeListValues.Columns[2].HeaderCell.Style.BackColor = _codeTable.Columns[0].Modified ? Color.LightGreen : Color.White;
            codeListValues.EnableHeadersVisualStyles = false;
            codeListValues.Rows.Add(_codeTable.Rows);
            for (int i = 0; i < _codeTable.Rows; i++)
            {
                var row = _converter.GetRow(_codeTable, i + 1);
                if (row != null)
                {
                    codeListValues[0, i].Value = row.SrcValues[C1];
                    codeListValues[1, i].Value = row.SrcValues[C2];
                    codeListValues[2, i].Value = row.DestValues[C1];
                    codeListValues[3, i].Value = row.DestValues[C2];
                    codeListValues[4, i].Value = codeListValues[5, i].Value = "0";
                    if (row.SrcValues[C1] != row.DestValues[C1])
                    {
                        codeListValues[0, i].Style.BackColor = Color.LightGreen;
                        codeListValues[2, i].Style.BackColor = Color.LightGreen;
                        codeListValues[4, i].Value = "1";
                    }
                    if (row.ErrorsColumns.Contains(C1))
                    {
                        codeListValues[0, i].Style.BackColor = Color.Red;
                        codeListValues[2, i].Style.BackColor = Color.Red;
                        codeListValues[5, i].Value = "1";
                    }
                }                
            }
            codeListValues.ClearSelection();
            tableErrorsLabel.Text = string.Format(TableErrorsLabel, _codeTable.Errors);
        }

        private void UpdateDataRow()
        {
            valueRichTextBox.Text = string.Empty;
            rowLabel.Text = string.Format(RowsLabel, _rowIndex, _mainTable.Rows);
            dataValues.Rows.Clear();            
            var row = _converter.GetRow(_mainTable, _rowIndex);
            dataValues.Rows.Add(_mainTable.Columns.Count);
            for (int i = 0; i < _mainTable.Columns.Count; i++)
            {                
                var column = _mainTable.Columns[i];
                dataValues[0, i].Value = column.Name;
                dataValues[1, i].Value = column.Description;
                dataValues[2, i].Value = column.TypeOriginal;
                if(column.Modified) { dataValues[2, i].Style.BackColor = Color.LightGreen; }
                if (row != null && row.SrcValues.ContainsKey(column.Id))
                {
                    dataValues[3, i].Value = row.SrcValues[column.Id];
                    if (row.SrcValues[column.Id] != row.DestValues[column.Id]) { dataValues[3, i].Style.BackColor = Color.LightGreen; }
                    if (row.ErrorsColumns.Contains(column.Id)) { dataValues[3, i].Style.BackColor = Color.Red; }                    
                }
                dataValues[4, i].Value = column.Type;
                if (column.Modified) { dataValues[4, i].Style.BackColor = Color.LightGreen; }
                if (row != null && row.SrcValues.ContainsKey(column.Id))
                {
                    dataValues[5, i].Value = row.DestValues[column.Id];
                    if (row.SrcValues[column.Id] != row.DestValues[column.Id]) { dataValues[5, i].Style.BackColor = Color.LightGreen; }
                    if (row.ErrorsColumns.Contains(column.Id)) { dataValues[5, i].Style.BackColor = Color.Red; }                    
                }
                dataValues[6, i].Value = column.Differences;
                dataValues[7, i].Value = column.ErrorsRows.Count > 0 ? column.ErrorsRows.Count : 0;
                if(column.ErrorsRows.Count > 0) { dataValues[7, i].Style.BackColor = Color.Red; }
            }
            tableErrorsLabel.Text = string.Format(TableErrorsLabel, _mainTable.Errors);
            if (row != null) { rowErrorsLabel.Text = string.Format(RowErrorsLabel, row.ErrorsColumns.Count); }
            dataValues.ClearSelection();
            if (_selectedColumn > -1)
            {
                dataValues.Rows[_selectedColumn].Cells[0].Selected = true;
                dataValues.FirstDisplayedScrollingRowIndex = _selectedColumn;
                UpdateErrorButton();
            }
        }

        private void UpdateErrorButton()
        {
           var errorRows = _mainTable.Columns[_selectedColumn].ErrorsRows;
           nextErrorButton.Enabled = errorRows.Any(index => (_rowIndex - 1) < index);
           prevErrorButton.Enabled = errorRows.Any(index => (_rowIndex - 1) > index);
           dataValues.Rows[_selectedColumn].DefaultCellStyle.Font = new Font(dataValues.Font, FontStyle.Bold);
        }       
    }
}
