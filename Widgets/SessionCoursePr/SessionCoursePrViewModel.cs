using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace SuperVision.Widgets.SessionCoursePr;

public partial class SessionCoursePrViewModel : WidgetViewModel
{
    public override string DisplayName => "Course PRs (Session)";
    public override string WidgetType => "SessionCoursePr";

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _sessionCoursePrs = $"Session:\n5lap: 0'00\"00\nFlap: 0'00\"00";

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        SessionCoursePrs = $"Session:\n5lap: {Globals.CsToStr(Globals.sessionData[Globals.currentCourse].FiveLap)}\nFlap: {Globals.CsToStr(Globals.sessionData[Globals.currentCourse].Flap)}";
    }
}
