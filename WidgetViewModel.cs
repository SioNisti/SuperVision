using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperVision.ViewModels;

public abstract partial class WidgetViewModel : ObservableObject, IWidget
{

    [ObservableProperty] private string _fontColor = "#FFFFFF";
    [ObservableProperty] private string _bgColor = "#000000";

    public virtual string DisplayName => GetType().Name.Replace("ViewModel", "");
    public abstract string WidgetType { get; }
    public abstract Dictionary<uint, uint> GetRequiredAddresses();
    public abstract void UpdateState(Dictionary<uint, byte[]> memoryData);
    public abstract void RefreshDisplay();
    public ObservableCollection<WidgetVariable> Variables { get; set; } = new();

    protected void DefineVariable(string name, string type, string defaultValue, List<string>? options = null)
    {
        Variables.Add(new WidgetVariable(name, type, defaultValue, options));
    }

    protected string GetVar(string name)
    {
        var v = Variables.FirstOrDefault(x => x.Name == name);
        return v?.Value ?? "";
    }

    protected bool GetBool(string name)
    {
        var val = GetVar(name);
        return bool.TryParse(val, out bool result) && result;
    }

    public void ApplySettings(WidgetSettings settings)
    {
        FontColor = settings.FontColor;
        BgColor = settings.BgColor;

        foreach (var variable in Variables)
        {
            if (settings.Variables.TryGetValue(variable.Name, out string? savedValue))
            {
                variable.Value = savedValue;
            }
        }

        RefreshDisplay();
    }
}