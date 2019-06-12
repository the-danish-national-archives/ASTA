using Rigsarkiv.Athena.Logging;
using Rigsarkiv.AthenaWPF;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace AthenaWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string DestFolderName = "AVID.SA.{0}.1";
        private StructureViewModel _viewModel = new StructureViewModel();
        private string _srcPath = null;
        private string _destPath = null;
        private string _destFolder = null;
        private LogManager _logManager = null;
        private Rigsarkiv.Athena.Converter _converter = null;
        private RichTextBox _outputRichTextBox = null;

        private List<LogEntity> _logEntities = null;
        private int i = 0;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
            var args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                _srcPath = args[1];
                if (args.Length == 3) { _destPath = args[2]; }
                var folderName = _srcPath.Substring(_srcPath.LastIndexOf("\\") + 1);
                folderName = folderName.Substring(0, folderName.LastIndexOf("."));
                _destFolder = string.Format(DestFolderName, folderName.Substring(3));
            }
            Update();
            _outputRichTextBox = (RichTextBox)FindName("outputRichTextBox");
        }

        private void Update()
        {
            _destPath = "xxx";
            var textBox = (TextBox)FindName("sipTextBox");
            textBox.Text = _srcPath;
            textBox = (TextBox)FindName("aipTextBox");
            textBox.Text = _destPath;
            textBox = (TextBox)FindName("aipNameTextBox");
            textBox.Text = _destFolder;
        }

        private void SipButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDlg = new WinForms.OpenFileDialog();
            openFileDlg.DefaultExt = ".json";
            openFileDlg.Filter = "SIP metadata (.json)|*.json";
            if(_srcPath != null) {
                openFileDlg.FileName = _srcPath.Substring(_srcPath.LastIndexOf("\\") + 1);
                openFileDlg.InitialDirectory = _srcPath.Substring(0, _srcPath.LastIndexOf("\\"));
            }
            var result = openFileDlg.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                _srcPath = openFileDlg.FileName;
                ((TextBox)FindName("sipTextBox")).Text = _srcPath;
            }
        }

        private void AipButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new WinForms.FolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;

            if (_destPath != null) { folderDialog.SelectedPath = _destPath; }
            var result = folderDialog.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                _destPath = folderDialog.SelectedPath;
                ((TextBox)FindName("aipTextBox")).Text = _destPath;
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            i = 0;
            _outputRichTextBox.Document.Blocks.Clear();
            _viewModel.Validate();
            if(_viewModel.HasErrors) { return; }

            _logEntities = new List<LogEntity>();            
            _logManager = new LogManager();
            _logManager.LogAdded += OnLogAdded;
            _converter = new Rigsarkiv.Athena.Structure(_logManager, _srcPath, _destPath, _destFolder);
            _converter.Run();
            
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Paragraph para = new Paragraph(new Run(_logEntities[i].Message));
            switch (_logEntities[i].Level)
            {
                case LogLevel.Error: para.Foreground = Brushes.Red; break;
                case LogLevel.Info: para.Foreground = Brushes.Black; break;
                case LogLevel.Warning: para.Foreground = Brushes.Yellow; break;
            }
            _outputRichTextBox.Document.Blocks.Add(para);
            i++;
            if (i >= _logEntities.Count) { ((DispatcherTimer)sender).Stop(); }
        }

        private void OnLogAdded(object arg1, LogEventArgs e)
        {
            _logEntities.Add(e.LogEntity);
        }
    }
}
