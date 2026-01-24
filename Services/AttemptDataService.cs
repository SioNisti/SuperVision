using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SuperVision.Services
{
    public class AttemptDataService : IWidget
    {
        public string WidgetType => "Internal_AttemptLogger";

        private bool _jsonLock = false;
        private int  _lastCountedLap = 0;

        public Dictionary<uint, uint> GetRequiredAddresses() => new() {
            { 0xF50F33, 30 }, // Lap times
            { 0xF510F9, 1 },  // Lap count
            { 0xF50101, 10 }, // Race timer
            { 0xF51012, 1 },  // Racer ID
            { 0xF5002C, 1 },  // Game Mode
            { 0xF50036, 1 },  // Screen Mode
            { 0xF50162, 1 },  // Pause Mode
            { 0xF50124, 1 } //current course
        };

        public void UpdateState(Dictionary<uint, byte[]> data)
        {
            //grab the data
            if (!data.TryGetValue(0xF50F33, out var lapData)) return;

            data.TryGetValue(0xF510F9, out var lapCountData);
            data.TryGetValue(0xF50101, out var timerData);
            data.TryGetValue(0xF51012, out var racerData);
            data.TryGetValue(0xF5002C, out var gModeData);
            data.TryGetValue(0xF50036, out var sModeData);
            data.TryGetValue(0xF50162, out var pModeData);
            data.TryGetValue(0xF50124, out var courseData);

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
            Globals.currentCourse = TrackNames.Map[courseData[0]];

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

            //constantly updates the best flap for the course
            var session = Globals.sessionData[Globals.currentCourse];
            if (lapSplits.Where(l => l > 0).ToList().Any())
            {
                int flap = lapSplits.Where(l => l > 0).ToList().Min();

                if (session.Flap == 0 || flap < session.Flap)
                {
                    session.Flap = flap;
                }
            }

            //session laps reached.
            if (lapReached >= 1 && lapReached <= 5)
            {
                if (_lastCountedLap != lapReached)
                {
                    session.LapsReached[lapReached - 1]++;
                    _lastCountedLap = lapReached;
                }
            } else
            {
                _lastCountedLap = 0;
            }
            /*
            for (int i = 1;i < 6;i++)
            {
                Debug.WriteLine($"L{i}: {session.LapsReached[i - 1]}");
            }*/
        }

        private void AddRaceToJson(string character, int racetime, int[] laps)
        {
            //SESSION
            var session = Globals.sessionData[Globals.currentCourse];
            session.Attempts++;

            var json = File.ReadAllText(Globals.jsonPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, CourseData>>>(json);

            int flap = laps.Where(l => l > 0).DefaultIfEmpty(0).Min();

            var courseData = data[Globals.currentRegion][Globals.currentCourse];
            var attempts = courseData.Attempts;
            courseData.Races.Add(new Race
            {
                Id = ++attempts,
                Character = character,
                Date = DateTime.Now,
                Racetime = racetime,
                Laps = laps
            });
            courseData.Attempts = attempts;

            //session save
            if (laps[4] > 0 && (racetime < session.FiveLap || session.FiveLap == 0))
            {
                session.FiveLap = racetime;
            }

            //check if race was finished to update count. and potential prs.
            if (laps[4] > 0)
            {
                courseData.Finishedraces++;
                session.FinishedRaces++;

                //update best lap splits
                for (int i = 0; i < courseData.Bestlaps.Length; i++)
                {
                    //if no best lap exists (=0) or is lower than new lap
                    if (courseData.Bestlaps[i] == 0)
                    {
                        courseData.Bestlaps[i] = attempts;
                    }
                    else if (laps[i] < courseData.Races[courseData.Bestlaps[i] - 1].Laps[i])
                    {
                        courseData.Bestlaps[i] = attempts;
                    }
                }

                //update fivelap
                if (courseData.Pr.Fivelap != 0)
                {
                    if (racetime < courseData.Races[courseData.Pr.Fivelap - 1].Racetime)
                    {
                        courseData.Pr.Fivelap = attempts;
                    }
                }
                else
                {
                    courseData.Pr.Fivelap = attempts;
                }

                //update flap
                if (courseData.Pr.Flap != 0)
                {
                    if (flap < courseData.Races[courseData.Pr.Flap - 1].Laps.Min())
                    {
                        courseData.Pr.Flap = attempts;
                    }
                }
                else
                {
                    courseData.Pr.Flap = attempts;
                }
            }

            json = JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(Globals.jsonPath, json);
        }
    }
}
