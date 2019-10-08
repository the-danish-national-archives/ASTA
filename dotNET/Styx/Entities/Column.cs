using System.Collections.Generic;

namespace Rigsarkiv.Styx.Entities
{
    /// <summary>
    /// Column Entity
    /// </summary>
    public class Column
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string TypeOriginal { get; set; }
        public bool Modified { get; set; }
        public string Highest { get; set; }
        public string Lowest { get; set; }
        public Dictionary<string,string> MissingValues { get; set; }
        public Table CodeList { get; set; }
        public string Message { get; set; }
    }
}
