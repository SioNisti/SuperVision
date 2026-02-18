using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace SuperVision.Widgets.AverageLap;

public partial class AverageLapViewModel : WidgetViewModel
{
    public override string DisplayName => "Average Current Lap";
    public override string WidgetType => "AverageLap";
    public AverageLapViewModel()
    {
        //define setting variable(s)
        DefineVariable("Comparison", "Combo", "Current Comparison", new List<string> { "Current Comparison", "All Time", "Session", "Grind" });
        DefineVariable("Prefix", "Text", "Average L{x}");
    }

    public override Dictionary<uint, uint> GetRequiredAddresses() => new()
    {
        { 0xF510F9, 1 }, //laps reached
    };

    [ObservableProperty] private string _widgetContentText = "";

    private int average = 0;
    private int lastlapreached = 0;
    private int lapReached = 1;

    public override void RefreshDisplay()
    {
        string prefix = GetVar("Prefix");

        if (prefix.Contains("{x}")) prefix = prefix.Replace("{x}", $"{lapReached}");
        prefix = Globals.handlePrefix(prefix);

        WidgetContentText = $"{prefix}{Globals.CsToStr(average)}";
    }
    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (!data.TryGetValue(0xF510F9, out var lapCountData)) return;

        lapReached = (lapCountData?[0] ?? 0) - 127;

        if (lapReached < 1 || lapReached > 5) return;
        if (lapReached == lastlapreached) return;
        lastlapreached = lapReached;

        if (!Globals.validateCourse(Globals.currentCourse)) return;

        string comparison = GetVar("Comparison");
        if (comparison == "Current Comparison") comparison = Globals.currentComparison;
        switch (comparison)
        {
            case "All Time":
                getAvrg(Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse]);
                break;

            case "Session":
                getAvrg(Globals.sessionData[Globals.currentCourse]);
                break;

            case "Grind":
                if (Globals.grindPath == "" || Globals.grindData == null) break;

                getAvrg(Globals.grindData);
                break;

            default:
                getAvrg(Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse]);
                break;
        }

        RefreshDisplay();
    }
    private void getAvrg(IRaceTracker data)
    {
        if (data == null || data.Races.Count < 1) return;

        int lapIndex = Math.Max(0, lapReached - 1);

        average = Convert.ToInt32(
            Math.Floor(data.Races
                .Select(r => r.Laps.ElementAtOrDefault(lapIndex))
                .Where(time => time > 0)
                .DefaultIfEmpty(0)
                .Average()
            )
        );
    }
}
