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
                File.WriteAllText(Globals.layoutPath, "[\r\n  { \"Type\": \"Splits\", \"FontColor\": \"white\", \"BgColor\": \"#000000\" }\r\n]");

            Dictionary<string, Dictionary<string, CourseData>> allData;

            if (File.Exists(Globals.jsonPath))
            {
                string json = File.ReadAllText(Globals.jsonPath);
                allData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, CourseData>>>(json) ?? new Dictionary<string, Dictionary<string, CourseData>>();

            }
            else
            {
                allData = new Dictionary<string, Dictionary<string, CourseData>>();
            }

            //check that all the courses are in the json. if not, add them.
            bool update = false;
            string[] regions = { "NTSC", "PAL" };

            foreach (string region in regions)
            {
                if (!allData.ContainsKey(region))
                {
                    allData[region] = new Dictionary<string, CourseData>();
                    update = true;
                }

                foreach (var course in Globals.courses)
                {
                    if (!allData[region].ContainsKey(course))
                    {
                        allData[region][course] = new CourseData
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
                string json = JsonSerializer.Serialize(allData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Globals.jsonPath, json);
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
