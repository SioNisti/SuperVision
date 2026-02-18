using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace SuperVision.Widgets.CoursePlaytime;

public partial class CoursePlaytimeViewModel : WidgetViewModel
{
    public override string DisplayName => "Course Playtime";
    public override string WidgetType => "CoursePlaytime";
    public CoursePlaytimeViewModel()
    {
        //define setting variable(s)
        DefineVariable("Comparison", "Combo", "Current Comparison", new List<string> { "Current Comparison", "All Time", "Session", "Grind" });
        DefineVariable("Prefix", "Text", "{course} Playtime");
    }

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _widgetContentText = "";
    private int _playtime = 0;

    public override void RefreshDisplay()
    {
        string prefix = GetVar("Prefix");
        prefix = Globals.handlePrefix(prefix);

        WidgetContentText = $"{prefix}{Globals.CsToStr(_playtime)}";
    }
    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (!Globals.validateCourse(Globals.currentCourse)) return;

        string comparison = GetVar("Comparison");
        if (comparison == "Current Comparison") comparison = Globals.currentComparison;
        switch (comparison)
        {
            case "All Time":
                getPlaytime(Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse]);
                break;

            case "Session":
                getPlaytime(Globals.sessionData[Globals.currentCourse]);
                break;

            case "Grind":
                if (Globals.grindPath == "" || Globals.grindData == null) break;

                getPlaytime(Globals.grindData);
                break;

            default:
                getPlaytime(Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse]);
                break;
        }

        RefreshDisplay();
    }

    private void getPlaytime(IRaceTracker data)
    {
        _playtime = 0;
        if (data == null || data.Races.Count < 1) return;

        //kinda bad to go through ALL races every 15ms
        for (int i = 0; i < data.Races.Count; i++)
        {
            _playtime += data.Races[i].Racetime;
        }
    }
}
