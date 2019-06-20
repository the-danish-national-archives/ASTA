using Rigsarkiv.Athena.Logging;
using System;

namespace Rigsarkiv.AthenaConsole
{
    class Program
    {
        private static LogManager _logManager = null;
        private static Athena.Converter _converter = null;
        static void Main(string[] args)
        {
            if (args != null && args.Length > 2)
            {
                _logManager = new LogManager();
                _logManager.LogAdded += OnLogAdded;
                _converter = new Athena.Structure(_logManager, args[0], args[1], args[2]);
                if(_converter.Run())
                {
                    _converter = new Athena.MetaData(_logManager, args[0], args[1], args[2]);
                    _converter.Run();
                }
            }
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
            System.Threading.Thread.Sleep(1000);
        }
    }
}
