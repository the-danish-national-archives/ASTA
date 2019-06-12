using Rigsarkiv.Athena.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rigsarkiv.Athena
{
    public class Converter
    {
        protected LogManager _logManager = null;
        protected string _srcPath = null;
        protected string _logSection = "";

        public Converter(LogManager logManager,string srcPath)
        {
            _logManager = logManager;
            _srcPath = srcPath;           
        }

        public virtual void Run()
        {

        }
    }
}
