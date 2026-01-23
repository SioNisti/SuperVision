using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperVision
{
    public class CourseData
    {
        public int Finishedraces { get; set; }
        public int Attempts { get; set; }
        public PersonalRecords Pr { get; set; } = new();
        public int[] Bestlaps { get; set; } = [0, 0, 0, 0, 0];
        public List<Race> Races { get; set; } = new();
    }

    public class PersonalRecords
    {
        public int Fivelap { get; set; }
        public int Flap { get; set; }
    }

    public class Race
    {
        public int Id { get; set; }
        public string Character { get; set; } = "";
        public DateTime Date { get; set; }
        public int Racetime { get; set; }
        public int[] Laps { get; set; } = [0, 0, 0, 0, 0];
    }
}
