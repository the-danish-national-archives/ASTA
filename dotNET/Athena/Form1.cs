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
        DummyData.FauxData data = new DummyData.FauxData();
        public Form1(string sourcePath)
        {
            InitializeComponent();

            PopulateTableBox();
            if (sourcePath != null)
            {
                textBox1.Text = sourcePath;
                folderBrowserDialog1.SelectedPath = sourcePath;
            }
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
            var keys = data.Tabl1.Keys.ToArray();
            this.tablesBox.Items.AddRange(new object[]
            {
                "Table1",
                "Table2"
            });
        }

        public void tablesBox_Click(object sender, EventArgs e)
        {
            Console.WriteLine("clicked");
            String selectedItem = tablesBox.SelectedItem.ToString();
            this.dataRows.Items.Clear();
            Dictionary<String, List<String>> tableData = null;
            if(selectedItem == "Table1")
            {
                tableData = data.Tabl1;
            }
            else
            {
                tableData = data.Tabl2;
            }

            foreach (var row in tableData)
            {
                this.dataRows.Items.Add(row.Key);
            }
            this.dataRows.SelectedIndex = 0;
            var dataList = tableData.Keys.ToList();
            var indexKey = dataList[0];
            dataValues.Rows.Clear();
            this.dataValues.Rows.Add(tableData[indexKey].Capacity - 1);
            var dataInput = tableData[indexKey];
            for (int i = 0; i < tableData[indexKey].Capacity - 1; i++)
            {
                this.dataValues[0, i].Value = dataInput[i];
                this.dataValues[1, i].Value = dataInput[i];
            }
        }

    }
}
