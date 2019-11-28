using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Styx;
using Rigsarkiv.Styx.Entities;
using Rigsarkiv.StyxForm.Extensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Rigsarkiv.StyxForm
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Form1));
        const string DestFolderName = "DIP.{0}";
        const string SrcFolderNamePattern = "^AVID.SA.([0-9]+).[0-9]$";
        const string LogPath = "{0}\\{1}_ASTA_konverteringslog.html";
        const string ReportPath = "{0}\\{1}_ASTA_konverteringsrapport.html";
        private LogManager _logManager = null;
        private Converter _converter = null;
        private Regex _srcFolderNameRegex = null;
        private string _logPath = null;

        /// <summary>
        /// Constructors
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        public Form1(string srcPath, string destPath)
        {
            InitializeComponent();
            _srcFolderNameRegex = new Regex(SrcFolderNamePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var scripts = Enum.GetValues(typeof(ScriptType)).Cast<ScriptType>();
            scriptTypeComboBox.Items.AddRange(scripts.Where(s => s != ScriptType.Xml).Select(s => s.ToString()).ToArray());
            scriptTypeComboBox.SelectedIndex = 0;
            sipTextBox.Text = destPath;
            aipTextBox.Text = srcPath;
            UpdateFolderName(aipTextBox.Text);
        }

        private string GetLogPath()
        {
            string result = null;
            try
            {
                var destFolderPath = string.Format("{0}\\ASTA_konverteringslog_{1}", sipTextBox.Text, sipNameTextBox.Text);
                if (Directory.Exists(destFolderPath)) { Directory.Delete(destFolderPath, true); }
                Directory.CreateDirectory(destFolderPath);
                result = destFolderPath;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to get log path", ex);
            }
            return result;
        }
        private void UpdateFolderName(string srcPath)
        {
            var index = srcPath.LastIndexOf("\\");
            if(index > -1)
            {
                var folderName = srcPath.Substring(index + 1);
                if(_srcFolderNameRegex.IsMatch(folderName))
                {
                    sipNameTextBox.Text = string.Format(DestFolderName, _srcFolderNameRegex.Match(folderName).Groups[1].Value);
                }                
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
            var path = string.Format(LogPath, _logPath, sipNameTextBox.Text);
            OpenFile(path);
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
            _logPath = GetLogPath();
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
                var tableIndexXDocument = _converter.TableIndexXDocument;
                var researchIndexXDocument = _converter.ResearchIndexXDocument;
                _converter = new MetaData(_logManager, srcPath, destPath, destFolder, _converter.Report) { TableIndexXDocument = tableIndexXDocument, ResearchIndexXDocument = researchIndexXDocument };
                if (_converter.Run())
                {
                    _converter = new Data(_logManager, srcPath, destPath, destFolder, _converter.Report);
                    if (_converter.Run() && ((Data)_converter).Flush(string.Format(ReportPath, _logPath, destFolder), destFolder))
                    {
                        reportButton.Enabled = true;
                        scriptLabel1.Text = string.Format(scriptLabel1.Text, destFolder);
                        scriptLabel1.Visible = true;
                        scriptLabel2.Visible = true;
                        scriptLabel3.Visible = true;
                    }
                }
            }
            var path = string.Format(LogPath, _logPath, sipNameTextBox.Text);
            if (_logManager.Flush(path, sipNameTextBox.Text, _converter.GetLogTemplate()))
            {
                logButton.Enabled = true;
            }
            Cursor.Current = Cursors.Default;
            
        }

        private void reportButton_Click(object sender, EventArgs e)
        {
            var path = string.Format(ReportPath, _logPath, sipNameTextBox.Text);
            Cursor.Current = Cursors.WaitCursor;
            OpenFile(path);
            Cursor.Current = Cursors.Default;
        }

        private bool OpenFile(string path)
        {
            var result = true;
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error(string.Format("Start Process {0} Failed", path), ex);
            }
            return result;
        }
    }
}
