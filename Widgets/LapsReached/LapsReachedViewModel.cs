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

    public partial class LapDisplayItem : ObservableObject
    {
        [ObservableProperty] private string _label = "Lx";
        [ObservableProperty] private string _value = "0";
    }

    public ObservableCollection<LapDisplayItem> LapRows { get; } = new();

    public LapsReachedViewModel()
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
        var courseData = Globals.AllTimeData[Globals.currentRegion][Globals.currentCourse];

        var laps = courseData.LapsReached;

        for (int i = 0; i < 5; i++)
        {
            if (i < laps.Length)
            {
                LapRows[i].Value = laps[i].ToString();
            }
        }
    }
}