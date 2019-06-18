using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
