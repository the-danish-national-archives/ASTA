using System.Collections.Generic;
using System.Linq.Expressions;

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
        public bool IsKey { get; set; }
        public bool Modified { get; set; }
        public string Highest { get; set; }
        public string Lowest { get; set; }
        public Dictionary<string,string> MissingValues { get; set; }
        public List<string> SortedMissingValues { get; set; }
        public Table CodeList { get; set; }
        public string Message { get; set; }
        public TruncatedRow TruncatedRow { get; set; }
        public int DescriptionLengthExceeded { get; set; }
    }
}
