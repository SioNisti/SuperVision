using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace SuperVision.ViewModels;

public abstract partial class WidgetViewModel : ObservableObject, IWidget
{

    [ObservableProperty] private string _fontColor = "#FFFFFF";
    [ObservableProperty] private string _bgColor = "#000000";

    public virtual string DisplayName => GetType().Name.Replace("ViewModel", "");
    public abstract string WidgetType { get; }
    public abstract Dictionary<uint, uint> GetRequiredAddresses();
    public abstract void UpdateState(Dictionary<uint, byte[]> memoryData);
}