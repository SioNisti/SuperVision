using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SuperVision.Widgets.LapsReached;

public partial class LapsReachedViewModel : WidgetViewModel
{
    public override string DisplayName => "Laps Reached";
    public override string WidgetType => "LapsReached";

    [ObservableProperty] private string _prefixString = "";
    public partial class LapDisplayItem : ObservableObject
    {
        [ObservableProperty] private string _label = "Lx";
        [ObservableProperty] private string _value = "0";
    }

    public ObservableCollection<LapDisplayItem> LapRows { get; } = new();

    public LapsReachedViewModel()
    {
        //define setting variable(s)
        DefineVariable("Prefix", "Text", "Reached");
        DefineVariable("Comparison", "Combo", "Current Comparison", new List<string> { "Current Comparison", "All Time", "Session", "Grind" });
        DefineVariable("Display", "Combo", "Values", new List<string> { "Values", "Absolute", "Survival" });

        for (int i = 1; i <= 5; i++)
        {
            LapRows.Add(new LapDisplayItem { Label = $"L{i}:" });
        }
    }

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    private int[] reached = [0, 0, 0, 0, 0];
    private int[] reachedPercent = [0, 0, 0, 0, 0];
    private int totalAttempts = 0;
    private int[] show = [0, 0, 0, 0, 0];

    private void CalculatePercentage(int id, int where)
    {
        if (reached[id - 1] > 0)
        {
            reachedPercent[id] = (reached[id] * 100) / where;
        }
        else
        {
            reachedPercent[id] = 0;
        }
    }

    public override void RefreshDisplay()
    {
        string prefix = Globals.handlePrefix(GetVar("Prefix"), false);
        PrefixString = $"{prefix}";
        
        if (GetVar("Display") != "Values") show = reachedPercent; else show = reached;

        if (totalAttempts > 0)
        {
            reachedPercent[0] = (reached[0] * 100) / totalAttempts;

            for (int i = 1; i < 5; i++)
            {
                switch (GetVar("Display"))
                {
                    case "Absolute":
                        CalculatePercentage(i, totalAttempts);
                        break;

                    case "Survival":
                        CalculatePercentage(i, reached[i - 1]);
                        break;

                    default:
                        reachedPercent[i] = 0;
                        break;
                }
            }
        } else
        {
            Array.Clear(reached);
            Array.Clear(reachedPercent);
            Array.Clear(show);
        }

        for (int i = 0; i < 5; i++)
        {
            if (i < show.Length)
            {
                var x = show[i].ToString();

                if (GetVar("Display") != "Values") x += "%";

                LapRows[i].Value = x;
            }
        }
    }
    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (!Globals.validateCourse(Globals.currentCourse)) return;

        string comparison = GetVar("Comparison");
        if (comparison == "Current Comparison") comparison = Globals.currentComparison;
        switch (comparison)
        {
            case "All Time":
                reached = Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse].LapsReached.ToArray();
                totalAttempts = Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse].Attempts;
                break;

            case "Session":
                reached = Globals.sessionData[Globals.currentCourse].LapsReached.ToArray();
                totalAttempts = Globals.sessionData[Globals.currentCourse].Attempts;
                break;

            case "Grind":
                if (Globals.grindData == null || Globals.grindPath == "")
                {
                    Array.Clear(reached);
                    Array.Clear(reachedPercent);
                    Array.Clear(show);
                    break;
                }
                reached = Globals.grindData.LapsReached.ToArray();
                totalAttempts = Globals.grindData.Attempts;
                break;
        }

        RefreshDisplay();
    }
}