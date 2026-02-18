using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperVision
{
    public class SessionData : IRaceTracker
    {
        public int Finishedraces { get; set; }
        public int Attempts { get; set; }
        public PersonalRecords Pr { get; set; } = new();
        public int[] Bestlaps { get; set; } = [0, 0, 0, 0, 0];
        public int[] LapsReached { get; set; } = [0, 0, 0, 0, 0];
        public List<Race> Races { get; set; } = new();
    }
}
