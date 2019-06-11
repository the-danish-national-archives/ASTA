using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            if (args != null && args.Length == 3)
            {
                _srcPath = args[1];
                _destPath = args[2];
            }
            var srcPathTextBox = (TextBox)this.FindName("srcPath");
            srcPathTextBox.Text = _srcPath;
        }
    }
}
