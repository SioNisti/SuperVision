using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperVision.Widgets.CoursePr;

public partial class CoursePrViewModel : WidgetViewModel
{
    public override string DisplayName => "Course PRs";
    public override string WidgetType => "CoursePr";
    public CoursePrViewModel()
    {
        //define setting variable(s)
        DefineVariable("Comparison", "Combo", "All Time", new List<string> { "All Time", "Session", "Grind" });
        DefineVariable("Prefix", "Text", "PR");
    }

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _coursePrs = "";

    public override void RefreshDisplay()
    {
        string comparison = GetVar("Comparison");
        string prefix = GetVar("Prefix");
        CoursePrs = $"{prefix}:\n5lap: {getPrInfo("5lap", comparison)}\nFlap: {getPrInfo("flap", comparison)}";
    }
    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        RefreshDisplay();
    }
    public string getPrInfo(string type, string comparison)
    {
        switch (comparison)
        {
            case "All Time":
                return getAT(type);

            case "Session":
                if (type == "flap")
                {
                    return Globals.CsToStr(Globals.sessionData[Globals.currentCourse].Flap);
                } else
                {
                    return Globals.CsToStr(Globals.sessionData[Globals.currentCourse].FiveLap);
                }

            case "Grind":
                return getGrind(type);

            default:
                return getAT(type);
        }
    }

    public string getAT(string type)
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
            }
            else
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

    public string getGrind(string type)
    {
        if (Globals.grindPath == "" || Globals.grindData == null) return Globals.CsToStr(0);
        var gdata = Globals.grindData;

        if (type == "flap")
        {
            return gdata.Pr.Flap > 0 ? Globals.CsToStr(Globals.getRaceById(gdata.Pr.Flap, gdata.Races).Laps.Min()) : "0'00\"00";
        } else
        {
            return gdata.Pr.Fivelap > 0 ? Globals.CsToStr(Globals.getRaceById(gdata.Pr.Fivelap, gdata.Races).Racetime) : "0'00\"00";
        }
    }
}
