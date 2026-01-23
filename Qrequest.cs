using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVision
{
    class Qrequest
    {
        public string Opcode { get; set; }
        public string Space { get; set; }
        public string[] Flags { get; set; }
        public string[] Operands { get; set; }
    }
}