using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SuperVision.Services
{
    public class GrindDataService : IWidget
    {
        public string WidgetType => "Internal_GrindAttemptLogger";

        private bool _jsonLock = false;
        private int  _lastCountedLap = 0;

        public Dictionary<uint, uint> GetRequiredAddresses() => new() {
            { 0xF50F33, 30 }, //Lap times
            { 0xF510F9, 1 },  //Lap count
            { 0xF50101, 10 }, //Race timer
            { 0xF51012, 1 },  //Racer ID
            { 0xF5002C, 1 },  //Game Mode
            { 0xF50036, 1 },  //Screen Mode
            { 0xF50162, 1 },  //Pause Mode
        };

        public void UpdateState(Dictionary<uint, byte[]> data)
        {
            //dont do anything if a grind isnt active
            if (!Globals.isGrinding) return;

            //grab the data
            if (!data.TryGetValue(0xF50F33, out var lapData)) return;

            data.TryGetValue(0xF510F9, out var lapCountData);
            data.TryGetValue(0xF50101, out var timerData);
            data.TryGetValue(0xF51012, out var racerData);
            data.TryGetValue(0xF5002C, out var gModeData);
            data.TryGetValue(0xF50036, out var sModeData);
            data.TryGetValue(0xF50162, out var pModeData);

            //lap times
            int cs1 = Globals.StrToCs(Globals.BytesToStr(lapData[0], lapData[1], lapData[3]));
            int cs2 = Globals.StrToCs(Globals.BytesToStr(lapData[6], lapData[7], lapData[9]));
            int cs3 = Globals.StrToCs(Globals.BytesToStr(lapData[12], lapData[13], lapData[15]));
            int cs4 = Globals.StrToCs(Globals.BytesToStr(lapData[18], lapData[19], lapData[21]));
            int cs5 = Globals.StrToCs(Globals.BytesToStr(lapData[24], lapData[25], lapData[27]));

            int[] lapSplits = {
                Math.Max(0, cs1),
                Math.Max(0, cs2 - cs1),
                Math.Max(0, cs3 - cs2),
                Math.Max(0, cs4 - cs3),
                Math.Max(0, cs5 - cs4)
            };

            int lapReached = (lapCountData?[0] ?? 0) - 127;
            string totalTimeStr = Globals.BytesToStr(timerData[0], timerData[1], timerData[3]);
            int gameMode = gModeData?[0] ?? 0;
            int screenMode = sModeData?[0] ?? 0;
            int pauseMode = pModeData?[0] ?? 0;
            byte racerId = racerData?[0] ?? 0;

            bool raceFinished = lapSplits[4] > 0 && lapReached == 6;
            bool shouldSave = gameMode == 0x04 && screenMode == 0x02 && (pauseMode == 0x03 || raceFinished);

            if (shouldSave && !_jsonLock)
            {
                _jsonLock = true;
                int finishTime = cs5 == 0 ? Globals.StrToCs(totalTimeStr) : cs5;

                AddRaceToJson(DriverNames.Map[racerId], finishTime, lapSplits);
            }
            else if (gameMode == 0x04 && screenMode == 0x02 && pauseMode == 0x00 && lapReached != 6 && _jsonLock)
            {
                _jsonLock = false;
            }
        }

        private void AddRaceToJson(string character, int racetime, int[] laps)
        {
            var json = File.ReadAllText(Globals.grindPath);
            var grindData = JsonSerializer.Deserialize<GrindData>(json);
            if (grindData == null) return;

            int flap = laps.Where(l => l > 0).DefaultIfEmpty(0).Min();

            var attempts = grindData.Attempts;
            grindData.Races.Add(new Race
            {
                Id = ++attempts,
                Character = character,
                Date = DateTime.Now,
                Racetime = racetime,
                Laps = laps
            });
            grindData.Attempts = attempts;

            //check if race was finished to update count. and potential prs.
            if (laps[4] > 0)
            {
                grindData.Finishedraces++;

                if (racetime <= grindData.GoalTime && grindData.GoalType == "5Lap")
                {
                    grindData.EndDate = DateTime.Now;
                    Globals.isGrinding = false;
                }
                if (flap <= grindData.GoalTime && grindData.GoalType == "Flap")
                {
                    grindData.EndDate = DateTime.Now;
                    Globals.isGrinding = false;
                }

                //update best lap splits
                for (int i = 0; i < grindData.Bestlaps.Length; i++)
                {
                    //if no best lap exists (=0) or is lower than new lap
                    if (grindData.Bestlaps[i] == 0)
                    {
                        grindData.Bestlaps[i] = attempts;
                    }
                    else if (laps[i] < grindData.Races[grindData.Bestlaps[i] - 1].Laps[i])
                    {
                        grindData.Bestlaps[i] = attempts;
                    }
                }

                //update fivelap
                if (grindData.Bests.Fivelap != 0)
                {
                    if (racetime < grindData.Races[grindData.Bests.Fivelap - 1].Racetime)
                    {
                        grindData.Bests.Fivelap = attempts;
                    }
                }
                else
                {
                    grindData.Bests.Fivelap = attempts;
                }

                //update flap
                if (grindData.Bests.Flap != 0)
                {
                    if (flap < grindData.Races[grindData.Bests.Flap - 1].Laps.Min())
                    {
                        grindData.Bests.Flap = attempts;
                    }
                }
                else
                {
                    grindData.Bests.Flap = attempts;
                }
            }

            json = JsonSerializer.Serialize(grindData, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(Globals.grindPath, json);
        }
    }
}
