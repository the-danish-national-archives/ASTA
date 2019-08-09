using log4net;
using Rigsarkiv.Asta.Logging;
using Rigsarkiv.Styx;
using Rigsarkiv.Styx.Entities;
using System;

namespace Rigsarkiv.StyxConsole
{
    class Program
    {
        protected static readonly ILog _log = log4net.LogManager.GetLogger(typeof(Program));
        private static Asta.Logging.LogManager _logManager = null;
        private static Styx.Converter _converter = null;
        static void Main(string[] args)
        {
            _log.Info("Start");
            if (args != null && args.Length > 2)
            {
                _logManager = new Asta.Logging.LogManager();
                _logManager.LogAdded += OnLogAdded;
                var srcPath = args[0];
                var destPath = args[1];
                var destFolder = args[2];
                _converter = new Structure(_logManager, srcPath, destPath, destFolder, ScriptType.SPSS);
                if (_converter.Run())
                {
                    var researchIndexXDocument = _converter.ResearchIndexXDocument;
                    _converter = new MetaData(_logManager, srcPath, destPath, destFolder, _converter.Report) { ResearchIndexXDocument = researchIndexXDocument };
                    if (_converter.Run())
                    {
                        _converter = new Data(_logManager, srcPath, destPath, destFolder, _converter.Report);
                        if (_converter.Run())
                        {
                            var path = string.Format("{0}\\{1}_log.html", destPath, destFolder);
                            if (_logManager.Flush(path, destFolder, _converter.GetLogTemplate()))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Log file at: {0}", path);
                                Console.ResetColor();
                            }
                        }                        
                    }
                }
            }
            _log.Info("End");
        }

        private static void OnLogAdded(object sender, LogEventArgs e)
        {
            switch (e.LogEntity.Level)
            {
                case LogLevel.Error:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.LogEntity.Message);
                    }
                    break;
                case LogLevel.Info:
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(e.LogEntity.Message);
                    }
                    break;
                case LogLevel.Warning:
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(e.LogEntity.Message);
                    }
                    break;
            }
            Console.ResetColor();
            System.Threading.Thread.Sleep(10);
        }
    }
}
