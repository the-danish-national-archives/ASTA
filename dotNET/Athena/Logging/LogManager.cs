using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rigsarkiv.Athena.Logging
{
    /// <summary>
    /// Log Manager
    /// </summary>
    public class LogManager
    {
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
            return true;
        }
    }
}
