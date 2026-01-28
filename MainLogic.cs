using Avalonia.Controls;
using SuperVision.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SuperVision
{
    public class MainLogic
    {
        //initializing variables
        public Snessocket? Snessocket { get; set; }
        public bool isAttached = false;

        //check if the json exists and that it's good.
        public void CheckJson()
        {
            if (!Directory.Exists(Globals.folder)) 
               Directory.CreateDirectory(Globals.folder);

            if (!Directory.Exists(Path.Combine(Globals.folder, "Grinds")))
                Directory.CreateDirectory(Path.Combine(Globals.folder, "Grinds"));

            if (!File.Exists(Globals.jsonPath))
                File.WriteAllText(Globals.jsonPath, "{}");

            if (!File.Exists(Globals.layoutPath))
                File.WriteAllText(Globals.layoutPath, "[\r\n  {\r\n    \"Type\": \"Splits\",\r\n    \"FontColor\": \"white\",\r\n    \"BgColor\": \"black\",\r\n    \"Variables\": {\r\n      \"Show Total\": \"True\"\r\n    }\r\n  }\r\n]");
            
            string fjson = File.ReadAllText(Globals.jsonPath);
            Globals.AllTimeData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, CourseData>>>(fjson) ?? new Dictionary<string, Dictionary<string, CourseData>>();

            //check that all the courses are in the json. if not, add them.
            bool update = false;
            string[] regions = { "NTSC", "PAL" };

            foreach (string region in regions)
            {
                if (!Globals.AllTimeData.ContainsKey(region))
                {
                    Globals.AllTimeData[region] = new Dictionary<string, CourseData>();
                    update = true;
                }

                foreach (var course in Globals.courses)
                {
                    if (!Globals.AllTimeData[region].ContainsKey(course))
                    {
                        Globals.AllTimeData[region][course] = new CourseData
                        {
                            Finishedraces = 0,
                            Attempts = 0,
                            Pr = new PersonalRecords { Fivelap = 0, Flap = 0 },
                            Bestlaps = [0, 0, 0, 0, 0],
                            Races = new List<Race>()
                        };
                        update = true;
                    }
                }
            }

            if (update)
            {
                Globals.saveData(Globals.jsonPath);
            }
        }

        public List<IWidget> ActiveWidgets { get; set; } = new();

        public void ReadMemory()
        {
            if (!isAttached) return;

            var masterAddressList = new Dictionary<uint, uint>();
            foreach (var widget in ActiveWidgets)
            {
                foreach (var req in widget.GetRequiredAddresses())
                {
                    if (!masterAddressList.ContainsKey(req.Key) || masterAddressList[req.Key] < req.Value)
                    {
                        masterAddressList[req.Key] = req.Value;
                    }
                }
            }

            var results = new Dictionary<uint, byte[]>();
            foreach (var entry in masterAddressList)
            {
                byte[] data = Snessocket.GetAddress(entry.Key, entry.Value);
                results[entry.Key] = data;
            }

            foreach (var widget in ActiveWidgets)
            {
                widget.UpdateState(results);
            }
        }
    }
}
