using System.Collections.Generic;

namespace Rigsarkiv.Athena.Entities
{
    /// <summary>
    /// Table Entity
    /// </summary>
    public class Table
    {
        public string SrcFolder { get; set; }
        public string Folder { get; set; }
        public string Name { get; set; }
        public int Rows { get; set; }
        public int RowsCounter { get; set; }
        public List<Table> CodeList { get; set; }
        public List<Column> Columns { get; set; }
        public int Errors { get; set; }        
        public Dictionary<string,string> Options { get; set; }
        public Dictionary<string,Row> ErrorsRows { get; set; }
    }
}
