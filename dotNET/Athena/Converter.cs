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
        private LogManager _logManager = null;
        private string _srcPath = null;
        private string _destPath = null;

        public Converter(LogManager logManager,string srcPath,string destPath)
        {
            _logManager = logManager;
            _srcPath = srcPath;
            _destPath = destPath;
        }

        public void Run()
        {
            
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = "", Message = "Start Convert" });
            for(var i =0; i < 10; i++)
            {
                var level = (i < 5) ? LogLevel.Info : LogLevel.Warning;
                _logManager.Add(new LogEntity() { Level = level, Section = "Structure", Message = "Processs ...." });
            }
            _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = "Data", Message = "Error ...." });
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = "", Message = "End Convert" });
        }
    }
}
