using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rigsarkiv.AthenaConsole
{
    class Program
    {
        private static string _srcPath = null;
        private static string _destPath = null;
        static void Main(string[] args)
        {
            if (args != null && args.Length == 1)
            {
                _srcPath = args[0];
                _destPath = args[1];
            }
        }
    }
}
