using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SuperVision.Widgets.Attempts;

public partial class AttemptsViewModel : WidgetViewModel
{
    public override string DisplayName => "Attempts";
    public override string WidgetType => "Attempts";
    public AttemptsViewModel()
    {
        //define setting variable(s)
        DefineVariable("Comparison", "Combo", "All Time", new List<string> { "All Time", "Session", "Grind" });
        DefineVariable("Prefix", "Text", "Tries");
    }

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _courseName = "";
    [ObservableProperty] private string _attemptRatio = "";

    private int _attempts = 0;
    private int _finishes = 0;

    public override void RefreshDisplay()
    {
        string prefix = GetVar("Prefix");
        CourseName = $"{prefix}:";
        AttemptRatio = $"{_finishes}/{_attempts}";
    }
    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        string comparison = GetVar("Comparison");
        switch (comparison)
        {
            case "All Time":
                _attempts = Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse].Attempts;
                _finishes = Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse].Finishedraces;
                break;

            case "Session":
                _attempts = Globals.sessionData[Globals.currentCourse].Attempts;
                _finishes = Globals.sessionData[Globals.currentCourse].FinishedRaces;
                break;

            case "Grind":
                if (!Globals.isGrinding) break;

                _attempts = Globals.grindData.Attempts;
                _finishes = Globals.grindData.Finishedraces;
                break;
        }

        RefreshDisplay();
    }
}
