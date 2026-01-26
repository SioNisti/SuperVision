using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
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

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _sobTimeSplits = "SoB:\n   L1 0'00\"00\n   L2 0'00\"00\n   L3 0'00\"00\n   L4 0'00\"00\n   L5 0'00\"00\nTOTAL 0'00\"00";

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        try
        {
            var json = File.ReadAllText(Globals.jsonPath);
            var alltimedata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, CourseData>>>(json);
            var courseData = alltimedata[Globals.currentRegion][Globals.currentCourse];

            int l1 = courseData.Bestlaps[0] == 0 ? 0 : courseData.Races[courseData.Bestlaps[0] - 1].Laps[0];
            int l2 = courseData.Bestlaps[1] == 0 ? 0 : courseData.Races[courseData.Bestlaps[1] - 1].Laps[1];
            int l3 = courseData.Bestlaps[2] == 0 ? 0 : courseData.Races[courseData.Bestlaps[2] - 1].Laps[2];
            int l4 = courseData.Bestlaps[3] == 0 ? 0 : courseData.Races[courseData.Bestlaps[3] - 1].Laps[3];
            int l5 = courseData.Bestlaps[4] == 0 ? 0 : courseData.Races[courseData.Bestlaps[4] - 1].Laps[4];

            int total = l1 + l2 + l3 + l4 + l5;

            SobTimeSplits = $"SoB:\n   L1 {Globals.CsToStr(l1)}\n   L2 {Globals.CsToStr(l2)}\n   L3 {Globals.CsToStr(l3)}\n   L4 {Globals.CsToStr(l4)}\n   L5 {Globals.CsToStr(l5)}\nTOTAL {Globals.CsToStr(total)}";
        } catch (Exception ex)
        {
            Debug.WriteLine($"{ex.Message}");
        }
    }
}
