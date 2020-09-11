using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rigsarkiv.Styx.Entities
{
    public class TruncatedRow
    {
        public int RowNo { get; set; }
        public int ByteLength { get; set; }
        public int NoOfTruncationsForVariable { get; set; }
    }
}
