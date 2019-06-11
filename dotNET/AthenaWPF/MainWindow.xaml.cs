using System;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace AthenaWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _srcPath = null;
        private string _destPath = null;
        public MainWindow()
        {
            InitializeComponent();
            var args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                _srcPath = args[1];
                if (args.Length == 3) { _destPath = args[2]; }
            }
            Update();
        }

        private void Update()
        {
            var pathTextBox = (TextBox)FindName("sipTextBox");
            pathTextBox.Text = _srcPath;
            pathTextBox = (TextBox)FindName("aipTextBox");
            pathTextBox.Text = _destPath;
        }

        private void SipButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDlg = new WinForms.OpenFileDialog();
            openFileDlg.DefaultExt = ".json";
            openFileDlg.Filter = "SIP metadata (.json)|*.json";
            if(_srcPath != null) {
                openFileDlg.FileName = _srcPath;
                openFileDlg.InitialDirectory = _srcPath;
            }
            var result = openFileDlg.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                _srcPath = openFileDlg.FileName;
                Update();
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
                Update();
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
