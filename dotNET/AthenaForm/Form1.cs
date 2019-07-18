using Rigsarkiv.Athena;
using Rigsarkiv.Athena.Extensions;
using Rigsarkiv.Athena.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Rigsarkiv.AthenaForm
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Form1));
        const string DestFolderName = "AVID.SA.{0}.1";
        private LogManager _logManager = null;
        private Converter _converter = null;
        private Form2 _form;

        /// <summary>
        /// Constructors
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        public Form1(string srcPath, string destPath)
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
            Cursor.Current = Cursors.WaitCursor;
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
                    var tableIndexXDocument = _converter.TableIndexXDocument;
                    var researchIndexXDocument = _converter.ResearchIndexXDocument;
                    _converter = new Data(_logManager, srcPath, destPath, destFolder, _converter.Report) { TableIndexXDocument = tableIndexXDocument, ResearchIndexXDocument = researchIndexXDocument }  ;
                    if (_converter.Run())
                    {
                        _form = new Form2(srcPath, destPath, destFolder, _logManager, _converter.Report,outputRichTextBox);
                        nextForm.Enabled = true;
                    }                    
                }
            }
            Cursor.Current = Cursors.Default;
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
            //System.Threading.Thread.Sleep(200);
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
            Cursor.Current = Cursors.WaitCursor;
            var path = string.Format("{0}\\{1}_log.html", aipTextBox.Text, aipNameTextBox.Text);
            if (_logManager.Flush(path, aipNameTextBox.Text))
            {
                try
                {
                    Process.Start(path);
                }
                catch (Exception ex)
                {
                    _log.Error(string.Format("Start Process {0} Failed", path), ex);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            _log.Info("Run");
        }
    }
}
