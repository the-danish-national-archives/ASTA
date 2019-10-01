using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rigsarkiv.Athena.Entities
{
    /// <summary>
    /// Column Entity
    /// </summary>
    public class Column
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string TypeOriginal { get; set; }
        public bool Nullable { get; set; }
        public bool HasSpecialNumeric { get; set; }
        public bool HasMissingValues { get; set; }
        public string RegExp { get; set; }
        public bool Modified { get; set; }
        public int Differences { get; set; }
        public int Errors { get; set; }
        public List<int> ErrorsRows { get; set; }
        public string CodeListName { get; set; }
    }
}
