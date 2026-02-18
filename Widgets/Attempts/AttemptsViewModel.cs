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
        DefineVariable("Comparison", "Combo", "Current Comparison", new List<string> { "Current Comparison", "All Time", "Session", "Grind" });
        DefineVariable("Prefix", "Text", "{course}");
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
        prefix = Globals.handlePrefix(prefix, false);

        CourseName = $"{prefix}";
        AttemptRatio = $"{_finishes}/{_attempts}";
    }
    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (!Globals.validateCourse(Globals.currentCourse)) return;

        string comparison = GetVar("Comparison");
        if (comparison == "Current Comparison") comparison = Globals.currentComparison;

        switch (comparison)
        {
            case "All Time":
                _attempts = Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse].Attempts;
                _finishes = Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse].Finishedraces;
                break;

            case "Session":
                _attempts = Globals.sessionData[Globals.currentCourse].Attempts;
                _finishes = Globals.sessionData[Globals.currentCourse].Finishedraces;
                break;

            case "Grind":
                if (Globals.grindData == null || Globals.grindPath == "")
                {
                    _attempts = 0;
                    _finishes = 0;
                    break;
                }

                _attempts = Globals.grindData.Attempts;
                _finishes = Globals.grindData.Finishedraces;
                break;
        }

        RefreshDisplay();
    }
}
