using System.Collections.Generic;

namespace Rigsarkiv.Athena.Entities
{
    /// <summary>
    /// Report
    /// </summary>
    public class Report
    {
        public int TablesCounter { get; set; }
        public int CodeListsCounter { get; set; }
        public List<Table> Tables { get; set; }

    }
}
