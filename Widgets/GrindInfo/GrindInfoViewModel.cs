using Avalonia.Controls;
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
    private string _course = "MC3";
    private string _type = "5lap";
    private string _region = "NTSC";
    private int _goal = 0;
    public override void RefreshDisplay()
    {
        if (!Globals.isGrinding)
        {
            GrindInfos = "Grind Status\nInactive";
        } else
        {
            GrindInfos = $"{_course} {_type} {_region}\nGoal: {Globals.CsToStr(_goal)}";
        }
    }

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (!Globals.isGrinding) return;

        _course = Globals.grindData.Course;
        _type = Globals.grindData.GoalType;
        _region = Globals.grindData.Region;
        _goal = Globals.grindData.GoalTime;

        RefreshDisplay();
    }
}
