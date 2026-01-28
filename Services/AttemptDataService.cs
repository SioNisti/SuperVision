using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SuperVision.Widgets.LapsReached;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using static System.Collections.Specialized.BitVector32;

namespace SuperVision.Services
{
    public class AttemptDataService : IWidget
    {
        public string WidgetType => "Internal_AttemptLogger";

        private bool _jsonLock = true;
        private int  _lastCountedLap = 0;

        public Dictionary<uint, uint> GetRequiredAddresses() => new() {
            { 0xF50F33, 30 }, //Lap times
            { 0xF510F9, 1 },  //Lap count
            { 0xF50101, 10 }, //Race timer
            { 0xF51012, 1 },  //Racer ID
            { 0xF5002C, 1 },  //Game Mode
            { 0xF50036, 1 },  //Screen Mode
            { 0xF50162, 1 },  //Pause Mode
            { 0xF50124, 1 }   //current course
        };

        public async void UpdateState(Dictionary<uint, byte[]> data)
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

            //check if the course is good (basically not a battle course)
            if (!Globals.validateCourse(Globals.currentCourse)) return;

            var session = Globals.sessionData[Globals.currentCourse];

            bool raceFinished = lapSplits[4] > 0 && lapReached == 6;
            bool shouldSave = gameMode == 0x04 && screenMode == 0x02 && (pauseMode == 0x03 || raceFinished);

            try
            {
                if (shouldSave && !_jsonLock)
                {
                    _jsonLock = true;
                    int finishTime = cs5 == 0 ? Globals.StrToCs(totalTimeStr) : cs5;

                    var alltimeCourse = Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse];

                    UpdateRaceStats(alltimeCourse, DriverNames.Map[racerId], finishTime, lapSplits, lapReached);

                    session.Attempts++;
                    if (lapSplits[4] > 0)
                    {
                        session.FinishedRaces++;
                        if (finishTime < session.FiveLap || session.FiveLap == 0) session.FiveLap = finishTime;
                    }

                    Globals.saveData(Globals.jsonPath);

                    //GRIND
                    if (Globals.isGrinding)
                    {
                        var grindData = Globals.grindData;

                        //make sure youre on the correct region and course
                        if (Globals.currentCourse != grindData.Course || Globals.currentRegion != grindData.Region) return;

                        UpdateRaceStats(grindData, DriverNames.Map[racerId], finishTime, lapSplits, lapReached);

                        if (grindData.GoalType == "Flap")
                        {
                            int flap = lapReached > 1 ? lapSplits.Where(l => l > 0).ToList().Min() : 0;
                            if (flap <= grindData.GoalTime && flap != 0)
                            {
                                grindData.EndDate = DateTime.Now;
                            }
                        }
                        if (grindData.GoalType == "5lap" && lapSplits[4] > 0 && finishTime <= grindData.GoalTime)
                        {
                            grindData.EndDate = DateTime.Now;
                        }

                        Globals.saveData(Globals.grindPath);

                        if (grindData.EndDate != null) Globals.isGrinding = false;
                    }
                }
                else if (gameMode == 0x04 && screenMode == 0x02 && pauseMode == 0x00 && lapReached != 6 && _jsonLock)
                {
                    _jsonLock = false;
                }
            } catch (Exception ex)
            {
                Debug.WriteLine(ex);
                /* crashes
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Error",
                    $"Error saving the race.\n{ex}",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error
                );
                await box.ShowAsync();*/
            }
            

            //constantly updates the best flap for the course
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
        }

        private void UpdateRaceStats(IRaceTracker data, string character, int racetime, int[] laps, int lapreached)
        {
            Debug.WriteLine(data);
            data.Attempts++;
            int raceId = data.Attempts;

            data.Races.Add(new Race
            {
                Id = raceId,
                Character = character,
                Date = DateTime.Now,
                Racetime = racetime,
                Laps = laps
            });

            //if race finished, set lapreached to 5
            if (lapreached == 6) lapreached--;
            Debug.WriteLine(lapreached);
            //loop through all laps finished
            for (int i = 0; i < lapreached; i++)
            {
                //increment counter
                data.LapsReached[i]++;
            }

            //go through all the laps
            for (int i = 0; i < data.Bestlaps.Length; i++)
            {
                //stop if a lap is 0
                if (laps[i] == 0) break;

                //if id is 0 (no best lap) or the laptime is lower than the saved time
                if (data.Bestlaps[i] == 0 || laps[i] < data.Races[data.Bestlaps[i] - 1].Laps[i])
                {
                    //save race id
                    data.Bestlaps[i] = raceId;
                }
            }

            //if lap5 split is longer than 0 (race finished)
            if (laps[4] > 0)
            {
                data.Finishedraces++;

                //if fivelap id is 0 (no saved time) or if racetime is lower than saved racetime
                if (data.Pr.Fivelap == 0 || racetime < Globals.getRaceById(data.Pr.Fivelap, data.Races).Racetime)
                {
                    //save race id
                    data.Pr.Fivelap = raceId;
                }

                //grab the fastest lap
                int flap = laps.Where(l => l > 0).DefaultIfEmpty(0).Min();
                //if flap id is 0 (no saved time) or if flap is lower than saved flap
                if (data.Pr.Flap == 0 || flap < Globals.getRaceById(data.Pr.Flap, data.Races).Laps.Min())
                {
                    //save race id
                    data.Pr.Flap = raceId;
                }
            }
        }
    }
}
