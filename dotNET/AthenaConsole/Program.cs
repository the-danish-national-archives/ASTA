using log4net;
using Rigsarkiv.Athena;
using Rigsarkiv.Athena.Logging;
using System;

namespace Rigsarkiv.AthenaConsole
{
    class Program
    {
        protected static readonly ILog _log = log4net.LogManager.GetLogger(typeof(Program));
        private static Athena.Logging.LogManager _logManager = null;
        private static Athena.Converter _converter = null;
        static void Main(string[] args)
        {
            _log.Info("Start");
            if (args != null && args.Length > 2)
            {
                _logManager = new Athena.Logging.LogManager();
                _logManager.LogAdded += OnLogAdded;
                var srcPath = args[0];
                var destPath = args[1];
                var destFolder = args[2];
                _converter = new Structure(_logManager, srcPath, destPath, destFolder);
                if(_converter.Run())
                {
                    _converter = new MetaData(_logManager, srcPath, destPath, destFolder);
                    if (_converter.Run())
                    {
                        var tableIndexXDocument = _converter.TableIndexXDocument;
                        var researchIndexXDocument = _converter.ResearchIndexXDocument;
                        _converter = new Data(_logManager, srcPath, destPath, destFolder, _converter.Report) { TableIndexXDocument = tableIndexXDocument, ResearchIndexXDocument = researchIndexXDocument };
                        if (_converter.Run())
                        {
                            _converter = new Index(_logManager, srcPath, destPath, destFolder, _converter.Report);
                            if (_converter.Run())
                            {
                                var path = string.Format("{0}\\{1}.html", destPath, destFolder);
                                if (_logManager.Flush(path))
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Log file at: {0}", path);
                                    Console.ResetColor();
                                }
                            }                            
                        }
                    }
                }
            }
            _log.Info("End");
        }

        private static void OnLogAdded(object sender, LogEventArgs e)
        {
            switch(e.LogEntity.Level)
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
            System.Threading.Thread.Sleep(200);
        }
    }
}
