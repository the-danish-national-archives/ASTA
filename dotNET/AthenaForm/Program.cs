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
        private static string _srcPath = null;
        private static StringBuilder _logs = new StringBuilder();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0) {
                _srcPath = args[0];
                Log(string.Format("Process Dataset: {0}", _srcPath));
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(_srcPath));
        }

        public static void Log(string text)
        {
            _logs.Append(text + Environment.NewLine);
            //Console.WriteLine(text + _seprator);
        }
    }
}
