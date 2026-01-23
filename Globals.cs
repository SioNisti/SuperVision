using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVision
{
    public static class Globals
    {
        public static string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SuperVision");
        public static string jsonPath = Path.Combine(folder, "data.json");
        public static string layoutPath = Path.Combine(folder, "layout.json");

        public static string[] courses = {
            "MC1", "DP1", "GV1", "BC1", "MC2",
            "CI1", "GV2", "DP2", "BC2", "MC3",
            "KB1", "CI2", "VL1", "BC3", "MC4",
            "DP3", "KB2", "GV3", "VL2", "RR"
        };

        public static string currentRegion { get; set; } = "NTSC";
        public static string currentCourse { get; set; } = "MC3";

        public static Dictionary<string, SessionData> sessionData = new();

        //thing to convert given value to 0 if it's 0xFF (empty lap time)
        public static int Normalize(int value)
        {
            return (value == 0xFF) ? 0 : value;
        }

        //convert the bytes into a nice string
        public static string BytesToStr(int cs, int s, int m)
        {
            int lapcs = Normalize(cs);
            int laps = Normalize(s);
            int lapm = Normalize(m);
            return $"{lapm:X}'{laps:X2}\"{lapcs:X2}";
        }

        //converts given string into centiseconds
        public static int StrToCs(string timeString)
        {
            int total = 0;

            var parts = timeString.Split('\'');
            int minutes = Int32.Parse(parts[0]);
            var secParts = parts[1].Split('"');
            int seconds = Int32.Parse(secParts[0]);
            int centiseconds = Int32.Parse(secParts[1]);

            total += minutes * 6000;      // 1 minute = 60s = 6000cs
            total += seconds * 100;       // 1 second = 100cs
            total += centiseconds;

            return total;
        }

        //convert centiseconds to a nice string
        public static string CsToStr(int cs)
        {
            int minutes = cs / 6000;
            int seconds = (cs / 100) % 60;
            int cs2 = cs % 100;

            return $"{minutes}'{seconds:00}\"{cs2:00}";
        }

        public static bool validateCourse(string course)
        {
            int pos = Array.IndexOf(Globals.courses, course);
            if (pos > -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
