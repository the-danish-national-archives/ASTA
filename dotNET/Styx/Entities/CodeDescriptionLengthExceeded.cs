using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rigsarkiv.Styx.Entities
{
    public class CodeDescriptionLengthExceeded
    {
        public string Code { get; set; }
        public int ByteLength { get; set; }
    }
}
