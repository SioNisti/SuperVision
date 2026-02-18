using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SuperVision.Widgets.Splits;

public partial class SplitsViewModel : WidgetViewModel
{
    public override string DisplayName => "Splits";
    public override string WidgetType => "Splits";

    [ObservableProperty] private string _prefixString = "";
    public partial class LapDisplayItem : ObservableObject
    {
        [ObservableProperty] private string _label = "Lx";
        [ObservableProperty] private string _value = Globals.CsToStr(0);
    }

    public ObservableCollection<LapDisplayItem> LapRows { get; } = new();
    public SplitsViewModel()
    {
        //define setting variable(s)
        DefineVariable("Prefix", "Text", "Live");

        for (int i = 0; i <= 5; i++)
        {
            if (i != 5)
            {
                LapRows.Add(new LapDisplayItem { Label = $"L{i+1}" });
            } else
            {
                LapRows.Add(new LapDisplayItem { Label = $"TOTAL" });
            }
        }
    }

    public override Dictionary<uint, uint> GetRequiredAddresses() => new()
    {
        { 0xF50F33, 30 }, //lap times
        { 0xF50101, 3 }, //total time
        { 0xF510F9, 1 }, //laps reached
    };

    public bool _clipBoardLock = false;
    private int[] laps = [0, 0, 0, 0, 0];
    private string total = Globals.CsToStr(0);
    public override void RefreshDisplay()
    {
        string prefix = Globals.handlePrefix(GetVar("Prefix"), false);
        PrefixString = $"{prefix}";

        for (int i = 0; i <= 5; i++)
        {
            if (i != 5)
            {
                var x = Globals.CsToStr(laps[i]);

                LapRows[i].Value = x;
            }
            else
            {
                LapRows[i].Value = total;
            }
        }
    }

    public async override void UpdateState(Dictionary<uint, byte[]> data)
    {
        if (!data.TryGetValue(0xF50F33, out var lapData)) return;
        data.TryGetValue(0xF50101, out var totaltimeData);
        data.TryGetValue(0xF510F9, out var lapsreachedData);

        int lapreached = lapsreachedData[0] - 127;

        //lap times
        int cs1 = Globals.StrToCs(Globals.BytesToStr(lapData[0], lapData[1], lapData[3]));
        int cs2 = Globals.StrToCs(Globals.BytesToStr(lapData[6], lapData[7], lapData[9]));
        int cs3 = Globals.StrToCs(Globals.BytesToStr(lapData[12], lapData[13], lapData[15]));
        int cs4 = Globals.StrToCs(Globals.BytesToStr(lapData[18], lapData[19], lapData[21]));
        int cs5 = Globals.StrToCs(Globals.BytesToStr(lapData[24], lapData[25], lapData[27]));

        string formatted5 = Globals.BytesToStr(lapData[24], lapData[25], lapData[27]);

        string totalTime = Globals.BytesToStr(totaltimeData[0], totaltimeData[1], totaltimeData[3]);

        int[] lapSplits = {
            Math.Max(0, cs1),
            Math.Max(0, cs2 - cs1),
            Math.Max(0, cs3 - cs2),
            Math.Max(0, cs4 - cs3),
            Math.Max(0, cs5 - cs4)
        };

        if (lapreached < 6 && formatted5 == "0'00\"00") formatted5 = totalTime;

        for (int i = 0; i < lapSplits.Length; i++)
        {
            laps[i] = lapSplits[i];
        }
        
        total = formatted5;

        if (lapSplits[4] > 0 && !_clipBoardLock)
        {
            _clipBoardLock = true;
            await CopyToClipboard($"{Globals.CsToStr(lapSplits[0])} {Globals.CsToStr(lapSplits[1])} {Globals.CsToStr(lapSplits[2])} {Globals.CsToStr(lapSplits[3])} {Globals.CsToStr(lapSplits[4])}");
        }
        else if (lapreached < 6)
        {
            _clipBoardLock = false;
        }

        RefreshDisplay();
    }
    public async Task CopyToClipboard(string text)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.MainWindow;

                if (window?.Clipboard is { } clipboard)
                {
                    await clipboard.SetTextAsync(text);
                }
            }
        });
    }
}
