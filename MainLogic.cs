using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SuperVision
{
    public class MainLogic
    {
        public Usb2Snes.Usb2Snes SnesSocket { get; set; }
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
                File.WriteAllText(Globals.layoutPath, "{\r\n  \"WindowWidth\": 192,\r\n  \"WindowHeight\": 300,\r\n  \"FontName\": \"19/02/2026 21.16.33\",\r\n  \"FontSize\": 678979083,\r\n  \"FontColor\": \"Aqua\",\r\n  \"BgColor\": \"Aqua\",\r\n  \"Widgets\": [\r\n    {\r\n      \"Type\": \"Splits\",\r\n      \"FontName\": \"Courier New\",\r\n      \"FontSize\": 22,\r\n      \"FontColor\": \"White\",\r\n      \"BgColor\": \"Black\",\r\n      \"Variables\": {\r\n        \"Prefix\": \"Live:\"\r\n      },\r\n      \"DisplayText\": \"\"\r\n    }\r\n  ]\r\n}");
            
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

        public async Task ReadMemory()
        {
            if (!isAttached) return;

            //get the addresses we want to read
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

            //read the addresses
            var results = new Dictionary<uint, byte[]>();
            foreach (var entry in masterAddressList)
            {
                byte[] data = await SnesSocket.GetAddress((int)entry.Key, (int)entry.Value);

                if (data != null) results[entry.Key] = data;
            }

            foreach (var widget in ActiveWidgets)
            {
                widget.UpdateState(results);
            }
        }
    }
}
