using System.Collections.Generic;

namespace Rigsarkiv.Athena.Entities
{
    /// <summary>
    /// Table Entity
    /// </summary>
    public class Table
    {
        public string Folder { get; set; }
        public string Name { get; set; }
        public List<Table> CodeList { get; set; }
        public List<Column> Columns { get; set; }
    }
}
