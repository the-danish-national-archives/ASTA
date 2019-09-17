using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Rigsarkiv.Styx.Entities
{
    /// <summary>
    /// Report
    /// </summary>
    public class Report
    {
        [ScriptIgnore]
        public ScriptType ScriptType { get; set; }
        public string ScriptTypeString { get { return ScriptType.ToString(); } }
        public int TablesCounter { get; set; }
        public int CodeListsCounter { get; set; }
        public List<Table> Tables { get; set; }

    }
}
