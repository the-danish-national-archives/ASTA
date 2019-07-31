using System.Collections.Generic;

namespace Rigsarkiv.Styx.Entities
{
    /// <summary>
    /// Table Entity
    /// </summary>
    public class Table
    {
        public string SrcFolder { get; set; }
        public string Folder { get; set; }
        public string Name { get; set; }
        public List<Column> Columns { get; set; }
    }
}
