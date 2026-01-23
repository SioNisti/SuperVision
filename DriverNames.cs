using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVision
{
    internal class DriverNames
    {
        public static readonly Dictionary<int, string> Map = new Dictionary<int, string>
        {
            [0x0] = "Mario",
            [0x2] = "Luigi",
            [0x4] = "Bowser",
            [0x6] = "Princess",
            [0x8] = "DK Jr.",
            [0xa] = "Koopa",
            [0xc] = "Toad",
            [0xe] = "Yoshi",
        };
    }
}
