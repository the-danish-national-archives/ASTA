﻿using Rigsarkiv.Athena;
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
                        _converter = new Data(_logManager, srcPath, destPath, destFolder, _converter.Tables) { TableIndexXDocument = tableIndexXDocument, ResearchIndexXDocument = researchIndexXDocument };
                        if (_converter.Run())
                        {
                            _converter = new Index(_logManager, srcPath, destPath, destFolder);
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