using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rigsarkiv.Athena
{
    //test
    //https://stackoverflow.com/questions/5282999/reading-csv-file-and-storing-values-into-an-array
    static class Program
    {
        private static readonly ILog _log = log4net.LogManager.GetLogger(typeof(Program));
        private static string _srcPath = null;
        private static string _destPath = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            _log.Info("Run");
            if (args != null && args.Length > 0)
            {
                _srcPath = args[0];
                if (args.Length == 2) { _destPath = args[1]; }                
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(_srcPath, _destPath));
        }
    }
}
