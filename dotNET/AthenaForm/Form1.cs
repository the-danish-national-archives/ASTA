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

namespace Rigsarkiv.Athena
{
    public partial class Form1 : Form
    {
        private LogManager _logManager = null;
        private Data _converter = null;
        public Form1(string srcPath, string destPath,string destFolder, LogManager logManager)
        {
            InitializeComponent();            
            _logManager = logManager;
            if (srcPath != null)
            {
                textBox1.Text = srcPath;
                _converter = new Data(logManager, srcPath, destPath, destFolder);
            }
            PopulateTableBox();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void PopulateTableBox()
        {
            var tables = _converter.GetTables(true);
            this.mainTablesListBox.Items.AddRange(tables.Keys.ToArray());
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

    }
}
