using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rigsarkiv.Asta.Logging
{
    /// <summary>
    /// Log Manager
    /// </summary>
    public class LogManager
    {
        protected static readonly ILog _log = log4net.LogManager.GetLogger(typeof(LogManager));
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
        /// <param name="name"></param>
        /// <param name="logTemplate"></param>
        public bool Flush(string path, string name, string logTemplate)
        {
            var result = true;
            try
            {
                _log.Info("Flush log");
                var content = new StringBuilder();
                var counter = 0;
                _entities.ForEach(e =>
                {
                    if (e.Level == LogLevel.Error) { counter++; }
                    content.AppendFormat(Span, e.Section, DateTime.Now.ToString("yyyyMMddHHmmss"), name, e.Level, e.Message, Environment.NewLine);
                });
                File.WriteAllText(path, string.Format(logTemplate, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), name, content.ToString(), counter));
            }
            catch (Exception ex)
            {
                result = false;
                _log.Error("Failed to Flush log", ex);
            }
            return result;
        }        
    }
}
