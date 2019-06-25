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
        public string Type { get; set; }
        public string TypeOriginal { get; set; }
        public bool Nullable { get; set; }
        public string RegExp { get; set; }
    }
}
