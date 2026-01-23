using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperVision
{
    public class SessionData
    {
        public int FiveLap { get; set; } = 0;
        public int Flap { get; set; } = 0;
        public int Attempts { get; set; } = 0;
        public int FinishedRaces { get; set; } = 0;
    }
}
