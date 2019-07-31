using System.Collections.Generic;

namespace Rigsarkiv.Styx.Entities
{
    /// <summary>
    /// Report
    /// </summary>
    public class Report
    {
        public ScriptType ScriptType { get; set; }
        public List<Table> Tables { get; set; }

    }
}
