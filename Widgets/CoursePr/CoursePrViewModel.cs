using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace SuperVision.Widgets.CoursePr;

public partial class CoursePrViewModel : WidgetViewModel
{
    public override string DisplayName => "Course PRs";
    public override string WidgetType => "CoursePr";

    public override Dictionary<uint, uint> GetRequiredAddresses()
    {
        return new Dictionary<uint, uint>(); //doesnt read memory
    }

    [ObservableProperty] private string _coursePrs = $"PR:\n5lap: 0'00\"00\nFlap: 0'00\"00";

    public override void UpdateState(Dictionary<uint, byte[]> data)
    {
        CoursePrs = $"PR:\n5lap: {get5lapPrInfo(Globals.currentCourse)}\nFlap: {getFlapPrInfo(Globals.currentCourse)}";
    }
    public string get5lapPrInfo(string course)
    {
        if (Globals.validateCourse(course))
        {
            var json = File.ReadAllText(Globals.jsonPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, CourseData>>>(json);
            var courseData = data[Globals.currentRegion][course];
            int id = courseData.Pr.Fivelap;
            Race prRace = courseData.Races.FirstOrDefault(r => r.Id == id);

            if (prRace == null) return "0'00\"00";

            return Globals.CsToStr(prRace.Racetime);
        }
        else
        {
            return "0'00\"00";
        }
    }

    public string getFlapPrInfo(string course)
    {
        if (Globals.validateCourse(course))
        {
            var json = File.ReadAllText(Globals.jsonPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, CourseData>>>(json);
            var courseData = data[Globals.currentRegion][course];
            int id = courseData.Pr.Flap;
            Race prRace = courseData.Races.FirstOrDefault(r => r.Id == id);
            if (prRace == null) return "0'00\"00";
            List<int> prLaps = prRace.Laps.ToList();

            return Globals.CsToStr(prLaps.Min());
        }
        else
        {
            return "0'00\"00";
        }
    }
}
