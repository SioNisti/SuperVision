using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace SuperVision.Widgets.GrindInfo;

public partial class GrindInfoViewModel : WidgetViewModel
{
    public override string DisplayName => "Grind Info";
    public override string WidgetType => "GrindInfo";

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _grindInfos = $"Grind Status\nInactive";

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (Globals.grindPath == "") return;

        var json = File.ReadAllText(Globals.grindPath);
        var gdata = JsonSerializer.Deserialize<GrindData>(json);

        GrindInfos = $"{gdata.Course} {gdata.GoalType} {gdata.Region}\nGoal: {Globals.CsToStr(gdata.GoalTime)}";
    }
}
