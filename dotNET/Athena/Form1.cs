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
            var data = new DummyData.FauxData();
            var keys = data.Tabl1.Keys.ToArray();
            this.tablesBox.Items.AddRange(new object[]
            {
                keys[0],
                keys[1],
                keys[2]
            });
        }

    }
}
