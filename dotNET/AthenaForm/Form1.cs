using Rigsarkiv.Athena.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rigsarkiv.Athena.Entities;

namespace Rigsarkiv.Athena
{
    public partial class Form1 : Form
    {
        const string RowsLabel = "Række {0} ud af {1}";
        const string CodeTableLabel = "Kodetabel: {0}";
        const string MainTableLabel = "Hovedtabel: {0}";
        private LogManager _logManager = null;
        private Data _converter = null;
        private List<Table> _tables = null;
        private Table _mainTable = null;
        private Table _codeTable = null;
        private int _rowIndex = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        /// <param name="logManager"></param>
        /// <param name="tables"></param>
        public Form1(string srcPath, string destPath,string destFolder, LogManager logManager, List<Table> tables)
        {
            InitializeComponent();            
            _logManager = logManager;
             _converter = new Data(logManager, srcPath, destPath, destFolder, tables);
            _tables = tables;
            mainTablesListBox.Items.AddRange(_tables.Select(t => t.Name).ToArray());
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
            _rowIndex = 1;
            rowLabel.Text = "";
            dataValues.Rows.Clear();            
            _codeTable = _mainTable.CodeList[codeTablesListBox.SelectedIndex];
            tableInfoLabel.Text = string.Format(CodeTableLabel, _codeTable.Name);
            if (_codeTable.Errors.HasValue)
            {
                UpdateDataRow(_codeTable);
                nextButton.Enabled = prevButton.Enabled = searchButton.Enabled = true;
                previewProgressBar.Value = previewProgressBar.Maximum;
                previewButton.Enabled = false;                
            }
            else
            {
                nextButton.Enabled = prevButton.Enabled = searchButton.Enabled = false;
                previewProgressBar.Value = 0;
                previewButton.Enabled = true;
            }
        }

        private void mainTablesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _rowIndex = 1;
            rowLabel.Text = "";
            nextButton.Enabled = prevButton.Enabled = searchButton.Enabled = false;
            dataValues.Rows.Clear();
            previewProgressBar.Value = 0;
            _codeTable = null;
            codeTablesListBox.Items.Clear();
            _mainTable = _tables[mainTablesListBox.SelectedIndex];
            if(_mainTable.CodeList != null && _mainTable.CodeList.Count > 0)
            {
                codeTablesListBox.Items.AddRange(_mainTable.CodeList.Select(t => t.Name).ToArray());
            }
            tableInfoLabel.Text = string.Format(MainTableLabel, _mainTable.Name);
            if (_mainTable.Errors.HasValue)
            {
                UpdateDataRow(_mainTable);
                nextButton.Enabled = prevButton.Enabled = searchButton.Enabled = true;
                previewProgressBar.Value = previewProgressBar.Maximum;
                previewButton.Enabled = false;
            }
            else
            {
                nextButton.Enabled = prevButton.Enabled = searchButton.Enabled = false;
                previewProgressBar.Value = 0;
                previewButton.Enabled = true;
            }
        }

        private void previewButton_Click(object sender, EventArgs e)
        {
            previewProgressBar.ResumeLayout();
            previewProgressBar.Minimum = 0;
            previewProgressBar.Step = 1;
            dataValues.Rows.Clear();
            var table = (_codeTable != null) ? _codeTable : _mainTable;
            if (!table.Errors.HasValue) { table.Errors = 0; }
            previewProgressBar.Maximum = table.Rows;

            for (int i = 1; i <= table.Rows; i++)
            {
                previewProgressBar.PerformStep();
                _converter.GetRow(table, i);
            }
            nextButton.Enabled = searchButton.Enabled = true;
            UpdateDataRow(table);
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            _rowIndex++;
            var table = (_codeTable != null) ? _codeTable : _mainTable;
            if (table.Rows >= _rowIndex)
            {
                UpdateDataRow(table);
            }
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
        
        private void UpdateDataRow(Table table)
        {
            rowLabel.Text = string.Format(RowsLabel, _rowIndex, table.Rows);
            dataValues.Rows.Clear();
            var row = _converter.GetRow(table, _rowIndex);
            dataValues.Rows.Add(table.Columns.Count);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                dataValues[0, i].Value = column.Name;
                dataValues[1, i].Value = column.TypeOriginal;
                if (row != null && row.SrcValues.ContainsKey(column.Id)) { dataValues[2, i].Value = row.SrcValues[column.Id]; }
                dataValues[3, i].Value = column.Type;
                if (row != null && row.SrcValues.ContainsKey(column.Id)) { dataValues[4, i].Value = row.DestValues[column.Id]; }
            }
        }
    }
}
