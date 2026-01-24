using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace SuperVision.Widgets.SessionLapsReached;

public partial class SessionLapsReachedViewModel : WidgetViewModel
{
    public override string DisplayName => "Laps Reached (Session)";
    public override string WidgetType => "SessionLapsReached";

    public partial class LapDisplayItem : ObservableObject
    {
        [ObservableProperty] private string _label = "Lx";
        [ObservableProperty] private string _value = "0";
    }

    public ObservableCollection<LapDisplayItem> LapRows { get; } = new();

    public SessionLapsReachedViewModel()
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
        var laps = Globals.sessionData[Globals.currentCourse].LapsReached;

        for (int i = 0; i < 5; i++)
        {
            if (i < laps.Count)
            {
                LapRows[i].Value = laps[i].ToString();
            }
        }
    }
}