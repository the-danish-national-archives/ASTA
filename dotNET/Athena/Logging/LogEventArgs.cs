using System;

namespace Rigsarkiv.Athena.Logging
{
    /// <summary>
    /// Log Event Args
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// Log Entity
        /// </summary>
        public LogEntity LogEntity { get; set; }
    }
}
