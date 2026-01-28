using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace SuperVision.Widgets.GrindAttempts;

public partial class GrindAttemptsViewModel : WidgetViewModel
{
    public override string DisplayName => "Attempts (Grind)";
    public override string WidgetType => "GrindAttempts";

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _attemptRatio = "0/0";

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (Globals.grindPath == "" || Globals.grindData == null) return;

        AttemptRatio = $"{Globals.grindData.Finishedraces}/{Globals.grindData.Attempts}";
    }
}
