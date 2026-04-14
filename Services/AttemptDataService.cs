using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
//using SuperVision.Widgets.LapsReached;
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
            { 0xF50F33, 30 }, //Lap times P1
            { 0xF510F9, 1 },  //Lap count P1
            { 0xF50F51, 30 }, //Lap times P2
            { 0xF511C1, 1 },  //Lap count P2
            { 0xF5002E, 1 },  //Map/Game orient: 2 = g/m, 4 = m/g
            { 0xF50101, 10 }, //Race timer
            { 0xF51012, 1 },  //Racer ID P1
            { 0xF51112, 1 },  //Racer ID P2
            { 0xF5002C, 1 },  //Game Mode
            { 0xF50036, 1 },  //Screen Mode
            { 0xF50162, 1 },  //Pause Mode
            { 0xF50124, 1 },  //current course
            { 0xF51F20, 5 }   //replay check (id = 0, 4)
        };

        public void UpdateState(Dictionary<uint, byte[]> data)
        {
            //grab the data
            if (!data.TryGetValue(0xF50124, out var courseData)) return;

            Globals.currentCourse = TrackNames.Map[courseData[0]];
            //check if the course is good (basically not a battle course)
            if (!Globals.validateCourse(Globals.currentCourse)) return;

            data.TryGetValue(0xF50101, out var timerData);
            data.TryGetValue(0xF5002E, out var screenData);
            data.TryGetValue(0xF5002C, out var gModeData);
            data.TryGetValue(0xF50036, out var sModeData);
            data.TryGetValue(0xF50162, out var pModeData);
            data.TryGetValue(0xF51F20, out var replayCheckData);


            int mapview = screenData?[0] ?? 0; //2 top game, 4 bottom game
            bool isP2 = (mapview == 4);

            int[] lapSplits = new int[5];
            int lap5time = 0;
            int lapReached = 0;
            byte racerId = 0;

            string totalTimeStr = Globals.BytesToStr(timerData[0], timerData[1], timerData[3]);
            int gameMode = gModeData?[0] ?? 0;
            int screenMode = sModeData?[0] ?? 0;
            int pauseMode = pModeData?[0] ?? 0;


            if (mapview == 2 || mapview == 4) { //top/bottom
                data.TryGetValue(isP2 ? 0xF50F51u : 0xF50F33u, out var lapData);
                data.TryGetValue(isP2 ? 0xF511C1u : 0xF510F9u, out var lapCountData);
                data.TryGetValue(isP2 ? 0xF51112u : 0xF51012u, out var racerData);

                //lap times
                int cs1 = Globals.StrToCs(Globals.BytesToStr(lapData[0], lapData[1], lapData[3]));
                int cs2 = Globals.StrToCs(Globals.BytesToStr(lapData[6], lapData[7], lapData[9]));
                int cs3 = Globals.StrToCs(Globals.BytesToStr(lapData[12], lapData[13], lapData[15]));
                int cs4 = Globals.StrToCs(Globals.BytesToStr(lapData[18], lapData[19], lapData[21]));
                int cs5 = Globals.StrToCs(Globals.BytesToStr(lapData[24], lapData[25], lapData[27]));

                lapSplits = new int[] {
                    Math.Max(0, cs1),
                    Math.Max(0, cs2 - cs1),
                    Math.Max(0, cs3 - cs2),
                    Math.Max(0, cs4 - cs3),
                    Math.Max(0, cs5 - cs4)
                };

                lapReached = (lapCountData?[0] ?? 0) - 127;
                racerId = racerData?[0] ?? 0;
                lap5time = cs5;
            
            } else if (mapview == 0) {
                //multiplayer
            }

            //check that youre in tt
            if (gameMode != 0x04) return;

            var session = Globals.sessionData[Globals.currentCourse];

            bool raceFinished = lapSplits[4] > 0 && lapReached == 6;
            bool shouldSave = screenMode == 0x02 && (pauseMode == 0x03 || raceFinished) && (replayCheckData[0] != 2 && replayCheckData[4] != 192);

            try
            {
                if (shouldSave && !_jsonLock)
                {
                    _jsonLock = true;
                    int finishTime = lap5time == 0 ? Globals.StrToCs(totalTimeStr) : lap5time;

                    var alltimeCourse = Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse];

                    UpdateRaceStats(alltimeCourse, DriverNames.Map[racerId], finishTime, lapSplits, lapReached);
                    UpdateRaceStats(session, DriverNames.Map[racerId], finishTime, lapSplits, lapReached, true);

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
                else if (screenMode == 0x02 && pauseMode == 0x00 && lapReached != 6 && _jsonLock)
                {
                    _jsonLock = false;
                }
            }
            catch (Exception ex)
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
        }

        private void UpdateRaceStats(IRaceTracker data, string character, int racetime, int[] laps, int lapreached, bool isSession = false)
        {
            data.Attempts++;
            int raceId = data.Attempts;
            bool isFinished = false;

            data.Races.Add(new Race
            {
                Id = raceId,
                Character = character,
                Date = DateTime.Now,
                Racetime = racetime,
                Laps = laps
            });

            //if race finished, set lapreached to 5
            if (lapreached == 6)
            {
                lapreached--;
                isFinished = true;
            }

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
                if (data.Bestlaps[i] == 0 || laps[i] < Globals.getRaceById(data.Bestlaps[i], data.Races).Laps[i])
                {
                    //save race id
                    data.Bestlaps[i] = raceId;
                }
            }

            //if finished OR isSession is true
            if (isFinished || isSession)
            {
                //grab the fastest non 0 lap
                int flap = laps[0] > 0 ? laps.Where(l => l > 0).DefaultIfEmpty(0).Min() : 0;
               
                //if fastest lap is 0 (no laps finished), stop.
                if (flap == 0) return;

                //if flap id is 0 (no saved time) or if flap is lower than saved flap
                if (data.Pr.Flap == 0 || flap < Globals.getRaceById(data.Pr.Flap, data.Races).Laps.Where(l => l > 0).DefaultIfEmpty(0).Min())
                {
                    //save race id
                    data.Pr.Flap = raceId;
                }
            }

            //if not finished, return.
            if (!isFinished) return;

            data.Finishedraces++;
            //if fivelap id is 0 (no saved time) or if racetime is lower than saved racetime
            if (data.Pr.Fivelap == 0 || racetime < Globals.getRaceById(data.Pr.Fivelap, data.Races).Racetime)
            {
                //save race id
                data.Pr.Fivelap = raceId;
            }
        }
    }
}
