using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace SuperVision.Widgets.CoursePr;

public partial class CoursePrViewModel : WidgetViewModel
{
    public override string DisplayName => "Course PRs";
    public override string WidgetType => "CoursePr";
    public CoursePrViewModel()
    {
        //define setting variable(s)
        DefineVariable("Comparison", "Combo", "Current Comparison", new List<string> { "Current Comparison", "All Time", "Session", "Grind" });
        DefineVariable("Prefix", "Text", "PR");
    }

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _labelPrefix = "";
    [ObservableProperty] private string _course5lap = "";
    [ObservableProperty] private string _courseFlap = "";

    public override void RefreshDisplay()
    {
        string comparison = GetVar("Comparison");
        string prefix = GetVar("Prefix");
        prefix = Globals.handlePrefix(prefix, false);

        LabelPrefix = $"{prefix}";
        Course5lap = getPrInfo("5lap", comparison);
        CourseFlap = getPrInfo("flap", comparison);
    }
    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        RefreshDisplay();
    }
    public string getPrInfo(string type, string comparison)
    {
        if (!Globals.validateCourse(Globals.currentCourse)) return Globals.CsToStr(0);

        if (comparison == "Current Comparison") comparison = Globals.currentComparison;
        switch (comparison)
        {
            case "All Time":
                return getAT(type);

            case "Session":
                return getSession(type);

            case "Grind":
                return getGrind(type);

            default:
                return getAT(type);
        }
    }

    public string getAT(string type)
    {
        var course = Globals.currentCourse;

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

    public string getSession(string type)
    {
        var course = Globals.currentCourse;

        var session = Globals.sessionData[course];
        int id = (type == "flap") ? session.Pr.Flap : session.Pr.Fivelap;
        Race prRace = Globals.getRaceById(id, session.Races);

        if (prRace == null) return "0'00\"00";

        int res = 0;
        if (type == "flap")
        {
            List<int> prLaps = prRace.Laps.ToList();
            res = prLaps.Where(l => l > 0).ToList().Min();
        }
        else
        {
            res = prRace.Racetime;
        }

        return Globals.CsToStr(res);
    }

    public string getGrind(string type)
    {
        if (Globals.grindPath == "" || Globals.grindData == null) return Globals.CsToStr(0);
        var gdata = Globals.grindData;

        if (type == "flap")
        {
            return gdata.Pr.Flap > 0 ? Globals.CsToStr(Globals.getRaceById(gdata.Pr.Flap, gdata.Races).Laps.Min()) : Globals.CsToStr(0);
        }
        else
        {
            return gdata.Pr.Fivelap > 0 ? Globals.CsToStr(Globals.getRaceById(gdata.Pr.Fivelap, gdata.Races).Racetime) : Globals.CsToStr(0);
        }
    }
}
