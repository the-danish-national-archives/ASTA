using Rigsarkiv.Athena.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Rigsarkiv.Athena.Entities;

namespace Rigsarkiv.Athena
{
    public partial class Form1 : Form
    {
        private LogManager _logManager = null;
        private Data _converter = null;
        private List<Table> _tables = null;
        private Table _mainTable = null;
        private Table _codeTable = null;
        private int _rowIndex = 1;
        public Form1(string srcPath, string destPath,string destFolder, LogManager logManager)
        {
            InitializeComponent();            
            _logManager = logManager;
            if (srcPath != null)
            {
                _converter = new Data(logManager, srcPath, destPath, destFolder);
            }
            _tables = _converter.GetTables();
            mainTablesListBox.Items.AddRange(_tables.Select(t => t.Name).ToArray());
        }

        public void tablesBox_Click(object sender, EventArgs e)
        {
            /*Console.WriteLine("clicked");
            String selectedItem = mainTablesListBox.SelectedItem.ToString();
            this.codeTablesListBox.Items.Clear();
            Dictionary<String, List<String>> tableData = null;
            if(selectedItem == "Table1")
            {
                //tableData = data.Tabl1;
            }
            else
            {
                //tableData = data.Tabl2;
            }

            foreach (var row in tableData)
            {
                //this.codeTablesListBox.Items.Add(row.Key);
            }
            this.codeTablesListBox.SelectedIndex = 0;
            var dataList = tableData.Keys.ToList();
            var indexKey = dataList[0];
            dataValues.Rows.Clear();
            this.dataValues.Rows.Add(tableData[indexKey].Capacity - 1);
            var dataInput = tableData[indexKey];
            for (int i = 0; i < tableData[indexKey].Capacity - 1; i++)
            {
                this.dataValues[0, i].Value = dataInput[i];
                this.dataValues[1, i].Value = dataInput[i];
            }*/
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
            dataValues.Rows.Clear();
            previewProgressBar.Value = 0;
            _codeTable = _mainTable.CodeList[codeTablesListBox.SelectedIndex];
        }

        private void mainTablesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataValues.Rows.Clear();
            previewProgressBar.Value = 0;
            _codeTable = null;
            codeTablesListBox.Items.Clear();
            _mainTable = _tables[mainTablesListBox.SelectedIndex];
            if(_mainTable.CodeList != null && _mainTable.CodeList.Count > 0)
            {
                codeTablesListBox.Items.AddRange(_mainTable.CodeList.Select(t => t.Name).ToArray());
            }
        }

        private void previewButton_Click(object sender, EventArgs e)
        {
            previewProgressBar.ResumeLayout();
            previewProgressBar.Minimum = 0;
            previewProgressBar.Step = 1;
            dataValues.Rows.Clear();
            var table = (_codeTable != null) ? _codeTable : _mainTable;
            previewProgressBar.Maximum = table.Rows;

            for (int i = 1; i <= table.Rows; i++)
            {
                previewProgressBar.PerformStep();
                _converter.GetRow(table, i);
                //System.Threading.Thread.Sleep(200);
            }
            var row = _converter.GetRow(table, _rowIndex); 
            dataValues.Rows.Add(table.Columns.Count);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                dataValues[0, i].Value = column.Name;
                dataValues[1, i].Value = column.TypeOriginal;
                if (row.SrcValues.ContainsKey(column.Id)) { dataValues[2, i].Value = row.SrcValues[column.Id]; }
                dataValues[3, i].Value = column.Type;
                if (row.SrcValues.ContainsKey(column.Id)) { dataValues[4, i].Value = row.DestValues[column.Id]; }
            }
        }
    }
}
