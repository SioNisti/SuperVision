using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace SuperVision.Widgets.SobSplits;

public partial class SobSplitsViewModel : WidgetViewModel
{
    public override string DisplayName => "SoB Splits";
    public override string WidgetType => "SobSplits";

    [ObservableProperty] private string _prefixString = "";
    public partial class LapDisplayItem : ObservableObject
    {
        [ObservableProperty] private string _label = "Lx";
        [ObservableProperty] private string _value = Globals.CsToStr(0);
    }

    public ObservableCollection<LapDisplayItem> LapRows { get; } = new();
    public SobSplitsViewModel()
    {
        //define setting variable(s)
        DefineVariable("Comparison", "Combo", "Current Comparison", new List<string> { "Current Comparison", "All Time", "Session", "Grind" });
        DefineVariable("Prefix", "Text", "SoB");

        for (int i = 0; i <= 5; i++)
        {
            if (i != 5)
            {
                LapRows.Add(new LapDisplayItem { Label = $"L{i + 1}" });
            }
            else
            {
                LapRows.Add(new LapDisplayItem { Label = $"TOTAL" });
            }
        }
    }

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _sobTimeSplits = "SoB:\n   L1 0'00\"00\n   L2 0'00\"00\n   L3 0'00\"00\n   L4 0'00\"00\n   L5 0'00\"00\nTOTAL 0'00\"00";

    private int[] laps = [0,0,0,0,0];
    private string total = Globals.CsToStr(0);

    public override void RefreshDisplay()
    {
        total = Globals.CsToStr(laps[0] + laps[1] + laps[2] + laps[3] + laps[4]);

        string prefix = Globals.handlePrefix(GetVar("Prefix"), false);
        PrefixString = $"{prefix}";

        for (int i = 0; i <= 5; i++)
        {
            if (i != 5)
            {
                var x = Globals.CsToStr(laps[i]);

                LapRows[i].Value = x;
            }
            else
            {
                LapRows[i].Value = total;
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
                var courseData = Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse];

                for (int i = 0; i < laps.Length; i++)
                {
                    
                    laps[i] = courseData.Bestlaps[i] == 0 ? 0 : Globals.getRaceById(courseData.Bestlaps[i], courseData.Races).Laps[i];
                }
                break;

            case "Session":
                var scourseData = Globals.sessionData[Globals.currentCourse];

                for (int i = 0; i < laps.Length; i++)
                {
                    laps[i] = scourseData.Bestlaps[i] == 0 ? 0 : Globals.getRaceById(scourseData.Bestlaps[i], scourseData.Races).Laps[i];
                }
                break;

            case "Grind":
                if (Globals.grindData == null || Globals.grindPath == "")
                {
                    Array.Clear(laps);
                    break;
                }

                var gcourseData = Globals.grindData;

                for (int i = 0; i < laps.Length; i++)
                {
                    laps[i] = gcourseData.Bestlaps[i] == 0 ? 0 : Globals.getRaceById(gcourseData.Bestlaps[i], gcourseData.Races).Laps[i];
                }
                break;
        }

        RefreshDisplay();
    }
}
