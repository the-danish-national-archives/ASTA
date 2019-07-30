using System;

namespace Rigsarkiv.Asta.Logging
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
