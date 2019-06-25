﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Rigsarkiv.Athena.Logging
{
    /// <summary>
    /// Log Manager
    /// </summary>
    public class LogManager
    {
        const string ResourceLogFile = "Rigsarkiv.Athena.Resources.log.html";
        const string Span = "<span id=\"{0}_{1}\" name=\"{2}\" class=\"{3}\">{4}</span><br/>{5}";
        private List<LogEntity> _entities = null;
        /// <summary>
        /// Events subscription end points
        /// </summary>
        public event EventHandler<LogEventArgs> LogAdded;

        /// <summary>
        /// constructor
        /// </summary>
        public LogManager()
        {
            _entities = new List<LogEntity>();
        }

        /// <summary>
        /// Add new log 
        /// </summary>
        /// <param name="entity"></param>
        public void Add(LogEntity entity)
        {
            _entities.Add(entity);
            var args = new LogEventArgs();
            args.LogEntity = entity;

            LogAdded?.Invoke(this, args);
        }

        /// <summary>
        /// flush and save log file
        /// </summary>
        /// <param name="path"></param>
        public bool Flush(string path)
        {
            var result = true;
            try
            {
                string data = GetLogTemplate();
                var name = path.Substring(path.LastIndexOf("\\") + 1);
                name = name.Substring(0, name.LastIndexOf("."));
                var content = new StringBuilder();
                var counter = 0;
                _entities.ForEach(e =>
                {
                    if (e.Level == LogLevel.Error) { counter++; }
                    content.AppendFormat(Span, e.Section, DateTime.Now.ToString("yyyyMMddHHmmss"), name, e.Level, e.Message, Environment.NewLine);
                });
                File.WriteAllText(path, string.Format(data, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), name, content.ToString(), counter));
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        private string GetLogTemplate()
        {
            string result = null;
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(ResourceLogFile))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }
    }
}