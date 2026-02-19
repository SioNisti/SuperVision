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
    public GrindInfoViewModel()
    {
        //define setting variable(s)
        DefineVariable("Prefix", "Text", "{grind_course} {grind_type} {grind_region}");
        DefineVariable("Alignment", "Combo", "Left", new List<string> { "Left", "Center", "Right" });
        DefineVariable("Single Line", "Bool", "False");
    }

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _grindInfos = "";
    [ObservableProperty] private string _contentTextAlignment = "";

    public override void RefreshDisplay()
    {
        string prefix = GetVar("Prefix");
        ContentTextAlignment = GetVar("Alignment");
        if (!Globals.isGrinding)
        {
            GrindInfos = GetBool("Single Line") ? "Grind Status: Inactive" : "Grind Status:\nInactive";
        } else
        {
            GrindInfos = GetBool("Single Line") ? $"{Globals.handlePrefix(prefix, false)} {Globals.CsToStr(Globals.grindData.GoalTime)}" : $"{Globals.handlePrefix(prefix)}{Globals.CsToStr(Globals.grindData.GoalTime)}";//$"{_course} {_type} {_region}\nGoal: {Globals.CsToStr(_goal)}";
        }
    }

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (!Globals.isGrinding)
        {
            if (Globals.grindData != null) if (Globals.grindData.EndDate == null) GrindInfos = GetBool("Single Line") ? "Grind Status: Inactive" : "Grind Status:\nInactive";

            //return;
        }


        RefreshDisplay();
    }
}
