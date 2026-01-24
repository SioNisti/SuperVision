using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace SuperVision.Widgets.SessionAttempts;

public partial class SessionAttemptsViewModel : WidgetViewModel
{
    public override string DisplayName => "Attempts (Session)";
    public override string WidgetType => "SessionAttempts";

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _attemptRatio = "0/0";

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        AttemptRatio = $"{Globals.sessionData[Globals.currentCourse].FinishedRaces}/{Globals.sessionData[Globals.currentCourse].Attempts}";
    }
}
