using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace SuperVision.Widgets.CoursePr;

public partial class CoursePrViewModel : WidgetViewModel
{
    public override string DisplayName => "Course PRs";
    public override string WidgetType => "CoursePr";

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _coursePrs = $"PR:\n5lap: 0'00\"00\nFlap: 0'00\"00";

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        CoursePrs = $"PR:\n5lap: {getPrInfo("5lap")}\nFlap: {getPrInfo("flap")}";
    }

    public string getPrInfo(string type)
    {
        var course = Globals.currentCourse;

        if (Globals.validateCourse(course))
        {
            var courseData = Globals.AllTimeData[Globals.currentRegion][course];
            int id = (type == "flap") ? courseData.Pr.Flap : courseData.Pr.Fivelap;
            Race prRace = Globals.getRaceById(id, courseData.Races);

            if (prRace == null) return "0'00\"00";

            int res = 0;
            if (type == "flap")
            {
                List<int> prLaps = prRace.Laps.ToList();
                res = prLaps.Min();
            } else
            {
                res = prRace.Racetime;
            }
            
            return Globals.CsToStr(res);
        }
        else
        {
            return "0'00\"00";
        }
    }
}
