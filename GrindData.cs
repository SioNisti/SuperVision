using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperVision
{
    public interface IRaceTracker
    {
        int Attempts { get; set; }
        int Finishedraces { get; set; }
        PersonalRecords Pr { get; set; }
        int[] Bestlaps { get; set; }
        int[] LapsReached { get; set; }
        List<Race> Races { get; set; }
    }
    public class GrindData : IRaceTracker
    {
        public required string Region { get; set; }
        public required string Course { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int GoalTime { get; set; }
        public required string GoalType { get; set; }
        public int Attempts { get; set; }
        public int Finishedraces { get; set; }
        public PersonalRecords Pr { get; set; } = new();
        public int[] Bestlaps { get; set; } = [0, 0, 0, 0, 0];
        public int[] LapsReached { get; set; } = [0, 0, 0, 0, 0];
        public List<Race> Races { get; set; } = new();
    }
}
