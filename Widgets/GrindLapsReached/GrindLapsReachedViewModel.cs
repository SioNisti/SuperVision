using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace SuperVision.Widgets.GrindLapsReached;

public partial class GrindLapsReachedViewModel : WidgetViewModel
{
    public override string DisplayName => "Laps Reached (Grind)";
    public override string WidgetType => "GrindLapsReached";

    public partial class LapDisplayItem : ObservableObject
    {
        [ObservableProperty] private string _label = "Lx";
        [ObservableProperty] private string _value = "0";
    }

    public ObservableCollection<LapDisplayItem> LapRows { get; } = new();

    public GrindLapsReachedViewModel()
    {
        for (int i = 1; i <= 5; i++)
        {
            LapRows.Add(new LapDisplayItem { Label = $"L{i}:" });
        }
    }

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (Globals.grindPath == "") return;

        var grindjson = File.ReadAllText(Globals.grindPath);
        var grinddata = JsonSerializer.Deserialize<GrindData>(grindjson);

        var laps = grinddata.LapsReached;

        for (int i = 0; i < 5; i++)
        {
            if (i < laps.Length)
            {
                LapRows[i].Value = laps[i].ToString();
            }
        }
    }
}