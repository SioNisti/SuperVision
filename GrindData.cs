using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperVision
{
    public class GrindData
    {
        public required string Region { get; set; }
        public required string Course { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int GoalTime { get; set; }
        public required string GoalType { get; set; }
        public int Attempts { get; set; }
        public int Finishedraces { get; set; }
        public Bests Bests { get; set; } = new();
        public int[] Bestlaps { get; set; } = [0, 0, 0, 0, 0];
        public List<Race> Races { get; set; } = new();
    }

    public class Bests
    {
        public int Fivelap { get; set; }
        public int Flap { get; set; }
    }
}
