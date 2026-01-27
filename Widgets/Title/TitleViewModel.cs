using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace SuperVision.Widgets.Title;

public partial class TitleViewModel : WidgetViewModel
{
    public override string DisplayName => "Attempts";
    public override string WidgetType => "Title";

    public override Dictionary<uint, uint> GetRequiredAddresses() => new()
    {
        { 0xF50124, 1 }
    };

    [ObservableProperty] private string _courseName = "MC3:";
    [ObservableProperty] private string _attemptRatio = "0/0";

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (data.TryGetValue(0xF50124, out var bytes) && bytes.Length > 0)
        {
            byte courseId = bytes[0];
            string course = TrackNames.Map[courseId];
            CourseName = $"{course}:";
            AttemptRatio = getAttempts(course);
        }
    }

    public string getAttempts(string course)
    {
        if (Globals.validateCourse(course))
        {
            return $"{Globals.AllTimeData[Globals.currentRegion][course].Finishedraces}/{Globals.AllTimeData[Globals.currentRegion][course].Attempts}";
        }
        else
        {
            return "0/0";
        }
    }
}
