using System.Collections.Generic;

namespace Rigsarkiv.Athena.Entities
{
    /// <summary>
    /// Row Entity
    /// </summary>
    public class Row
    {
        public Dictionary<string, string> SrcValues { get; set; }
        public Dictionary<string, string> DestValues { get; set; }
        public List<string> ErrorsColumns { get; set; }
    }
}
