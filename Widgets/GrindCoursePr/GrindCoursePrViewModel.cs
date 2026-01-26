using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace SuperVision.Widgets.GrindCoursePr;

public partial class GrindCoursePrViewModel : WidgetViewModel
{
    public override string DisplayName => "Course PRs (Grind)";
    public override string WidgetType => "GrindCoursePr";

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _grindCoursePrs = $"Grind Bests:\n5lap: 0'00\"00\nFlap: 0'00\"00";

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (Globals.grindPath == "") return;

        try
        {
            var json = File.ReadAllText(Globals.grindPath);
            var gdata = JsonSerializer.Deserialize<GrindData>(json);

            string flap = gdata.Pr.Flap > 0 ? Globals.CsToStr(gdata.Races[gdata.Pr.Flap - 1].Laps.Min()) : "0'00\"00";
            string fivelap = gdata.Pr.Fivelap > 0 ? fivelap = Globals.CsToStr(gdata.Races[gdata.Pr.Fivelap - 1].Racetime) : "0'00\"00";

            GrindCoursePrs = $"Grind Bests:\n5lap: {fivelap}\nFlap: {flap}";
        } catch (Exception e)
        {
            Debug.Write(e);
        }
    }
}
