using Rigsarkiv.Athena.Extensions;
using Rigsarkiv.Athena.Logging;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rigsarkiv.Athena
{
    public partial class Form2 : Form
    {
        const string DestFolderName = "AVID.SA.{0}.1";
        private LogManager _logManager = null;
        private Converter _converter = null;
        private Form1 _form;

        /// <summary>
        /// Constructors
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        public Form2(string srcPath, string destPath)
        {
            InitializeComponent();
            sipTextBox.Text = srcPath;
            aipTextBox.Text = destPath;
            var folderName = srcPath.Substring(srcPath.LastIndexOf("\\") + 1);
            folderName = folderName.Substring(0, folderName.LastIndexOf("."));
            aipNameTextBox.Text = string.Format(DestFolderName, folderName.Substring(3));
        }

        private void sipButton_Click(object sender, EventArgs e)
        {
            var openFileDlg = new OpenFileDialog();
            openFileDlg.DefaultExt = ".json";
            openFileDlg.Filter = "SIP metadata (.json)|*.json";
            if (!string.IsNullOrEmpty(sipTextBox.Text))
            {
                openFileDlg.FileName = sipTextBox.Text.Substring(sipTextBox.Text.LastIndexOf("\\") + 1);
                openFileDlg.InitialDirectory = sipTextBox.Text.Substring(0, sipTextBox.Text.LastIndexOf("\\"));
            }
            var result = openFileDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                sipTextBox.Text = openFileDlg.FileName;
            }
        }

        private void aipButton_Click(object sender, EventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;

            if (!string.IsNullOrEmpty(aipTextBox.Text)) { folderDialog.SelectedPath = aipTextBox.Text; }
            var result = folderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                aipTextBox.Text = folderDialog.SelectedPath;
            }
        }

        private void convertButton_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) { return; }
            outputRichTextBox.Clear();
            _logManager = new LogManager();
            _logManager.LogAdded += OnLogAdded;
            var srcPath = sipTextBox.Text;
            var destPath = aipTextBox.Text;
            var destFolder = aipNameTextBox.Text;

            _converter = new Structure(_logManager, srcPath, destPath, destFolder);
            if (_converter.Run())
            {
                _converter = new MetaData(_logManager, srcPath, destPath, destFolder);
                if(_converter.Run())
                {
                    _converter = new Data(_logManager, srcPath, destPath, destFolder, _converter.Tables);
                    if (_converter.Run())
                    {
                        _form = new Form1(srcPath, destPath, destFolder, _logManager, _converter.Tables);
                        nextForm.Enabled = true;
                    }                    
                }
            }
            logButton.Enabled = true;
        }

        private void OnLogAdded(object sender, LogEventArgs e)
        {
            var message = e.LogEntity.Message;
            switch (e.LogEntity.Level)
            {
                case LogLevel.Error: outputRichTextBox.AppendText(message, Color.Red); break;
                case LogLevel.Info: outputRichTextBox.AppendText(message, Color.Black); break;
                case LogLevel.Warning: outputRichTextBox.AppendText(message, Color.Orange); break;
            }
            System.Threading.Thread.Sleep(200);
        }

        private bool ValidateInputs()
        {
            var result = true;            
            if (string.IsNullOrEmpty(sipTextBox.Text))
            {
                sipPathRequired.SetError(sipTextBox, "Mappe mangler");
                result = false;
            }
            else
            {
                sipPathRequired.SetError(sipTextBox, string.Empty);
            }
            if (string.IsNullOrEmpty(aipNameTextBox.Text))
            {
                aipNameRequired.SetError(aipNameTextBox, "Navn mangler");
                result = false;
            }
            else
            {
                aipNameRequired.SetError(aipNameTextBox, string.Empty);
            }
            if (string.IsNullOrEmpty(aipTextBox.Text))
            {
                aipPathRequired.SetError(aipTextBox, "Mappe mangler");
                result = false;
            }
            else
            {
                aipPathRequired.SetError(aipTextBox, string.Empty);
            }
            return result;
        }

        private void nextForm_Click(object sender, EventArgs e)
        {
            _form.Show();
            this.Hide();
        }

        private void logButton_Click(object sender, EventArgs e)
        {
            var path = string.Format("{0}\\{1}.html", aipTextBox.Text, aipNameTextBox.Text);
            if (_logManager.Flush(path))
            {
                System.Diagnostics.Process.Start(path);
            }
        }
    }
}
