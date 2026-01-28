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
        DefineVariable("Comparison", "Combo", "All Time", new List<string> { "All Time", "Session", "Grind" });
        DefineVariable("Prefix", "Text", "Reached");

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

    public override void RefreshDisplay()
    {
        PrefixString = $"{GetVar("Prefix")}:";
        for (int i = 0; i < 5; i++)
        {
            if (i < reached.Length)
            {
                LapRows[i].Value = reached[i].ToString();
            }
        }
    }
    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (!Globals.validateCourse(Globals.currentCourse)) return;

        string comparison = GetVar("Comparison");
        switch (comparison)
        {
            case "All Time":
                reached = Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse].LapsReached;
                break;

            case "Session":
                reached = Globals.sessionData[Globals.currentCourse].LapsReached.ToArray();
                break;

            case "Grind":
                if (!Globals.isGrinding) break;
                reached = Globals.grindData.LapsReached;
                break;

            default:
                break;
        }

        RefreshDisplay();
    }
}