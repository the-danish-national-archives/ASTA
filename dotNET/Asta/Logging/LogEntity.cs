namespace Rigsarkiv.Asta.Logging
{
    /// <summary>
    /// Log Entity
    /// </summary>
    public class LogEntity
    {
        /// <summary>
        /// Log Level
        /// </summary>
        public LogLevel Level { get;set; }
        /// <summary>
        /// Section (structure or Data
        /// </summary>
        public string Section { get; set; }
        /// <summary>
        /// message text
        /// </summary>
        public string Message { get; set; }
    }
}
