using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Styx;
using Rigsarkiv.Styx.Entities;
using Rigsarkiv.StyxForm.Extensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Rigsarkiv.StyxForm
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Form1));
        const string DestFolderName = "FD.{0}";
        private LogManager _logManager = null;
        private Converter _converter = null;

        /// <summary>
        /// Constructors
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        public Form1(string srcPath, string destPath)
        {
            InitializeComponent();
            var scripts = Enum.GetValues(typeof(ScriptType)).Cast<ScriptType>();
            scriptTypeComboBox.Items.AddRange(scripts.Select(s => s.ToString()).ToArray());
            scriptTypeComboBox.SelectedIndex = 0;
            sipTextBox.Text = destPath;
            aipTextBox.Text = srcPath;
            UpdateFolderName(aipTextBox.Text);
        }

        private void UpdateFolderName(string srcPath)
        {
            var index = srcPath.LastIndexOf("\\AVID.SA.");
            if(index > -1)
            {
                var folderName = srcPath.Substring(index + 9);
                sipNameTextBox.Text = string.Format(DestFolderName, folderName);
            }           
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

        private void Form1_Shown(object sender, EventArgs e)
        {
            _log.Info("Run");
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
                UpdateFolderName(aipTextBox.Text);
            }
        }

        private void sipButton_Click(object sender, EventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;

            if (!string.IsNullOrEmpty(sipTextBox.Text)) { folderDialog.SelectedPath = sipTextBox.Text; }
            var result = folderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                sipTextBox.Text = folderDialog.SelectedPath;
            }
        }

        private void logButton_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var path = string.Format("{0}\\{1}_log.html", sipTextBox.Text, sipNameTextBox.Text);
            if (_logManager.Flush(path, sipNameTextBox.Text, _converter.GetLogTemplate()))
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
            if (string.IsNullOrEmpty(sipNameTextBox.Text))
            {
                sipNameRequired.SetError(sipNameTextBox, "Navn mangler");
                result = false;
            }
            else
            {
                sipNameRequired.SetError(sipNameTextBox, string.Empty);
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

        private void convertButton_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) { return; }
            Cursor.Current = Cursors.WaitCursor;
            outputRichTextBox.Clear();
            _logManager = new LogManager();
            _logManager.LogAdded += OnLogAdded;
            var srcPath = aipTextBox.Text;
            var destPath = sipTextBox.Text;
            var destFolder = sipNameTextBox.Text;
            var scriptType = (ScriptType)Enum.Parse(typeof(ScriptType), scriptTypeComboBox.SelectedItem.ToString(), true);
            _converter = new Structure(_logManager, srcPath, destPath, destFolder, scriptType);
            if (_converter.Run())
            {
                _converter = new MetaData(_logManager, srcPath, destPath, destFolder, _converter.Report);
                if (_converter.Run())
                {
                    _converter = new Data(_logManager, srcPath, destPath, destFolder, _converter.Report);
                    if (_converter.Run())
                    {

                    }
                }
            }
            Cursor.Current = Cursors.Default;
            logButton.Enabled = true;
        }
    }
}
