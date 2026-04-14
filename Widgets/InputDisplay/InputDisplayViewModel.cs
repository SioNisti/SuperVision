using CommunityToolkit.Mvvm.ComponentModel;
using SuperVision.ViewModels;
using System.Collections.Generic;

namespace SuperVision.Widgets.InputDisplay
{
    public partial class InputDisplayViewModel : WidgetViewModel
    {
        public override string DisplayName => "Input Display";
        public override string WidgetType => "InputDisplay";

        public override Dictionary<uint, uint> GetRequiredAddresses() => new() {
            { 0xF510C4, 2 },    //this and the next address hold the inputs in binary
            { 0xF511C4, 2 },    //p2
            { 0xF5002E, 1 }     //Map/Game orient: 2 = g/m, 4 = m/g
        };

        [ObservableProperty] private bool _btnA;
        [ObservableProperty] private bool _btnB;
        [ObservableProperty] private bool _btnX;
        [ObservableProperty] private bool _btnY;
        [ObservableProperty] private bool _btnL;
        [ObservableProperty] private bool _btnR;
        [ObservableProperty] private bool _btnStart;
        [ObservableProperty] private bool _btnSelect;
        [ObservableProperty] private bool _dUp;
        [ObservableProperty] private bool _dDown;
        [ObservableProperty] private bool _dLeft;
        [ObservableProperty] private bool _dRight;

        public InputDisplayViewModel()
        {
            //define setting variable(s)
            DefineVariable("Player", "Combo", "Auto", new List<string> { "Auto", "P1", "P2" });
        }

        public override void UpdateState(Dictionary<uint, byte[]> data)
        {
            string player = GetVar("Player");

            if (!data.TryGetValue(0xF5002E, out var screenData)) return;

            int mapview = screenData?[0] ?? 0; //2 top game, 4 bottom game
            bool usePlayer2 = (player == "P2") || (player == "Auto" && mapview == 4);

            if (!data.TryGetValue(usePlayer2 ? 0xF511C4u : 0xF510C4u, out var buffer)) return;
            if (buffer == null || buffer.Length < 2) return;

            byte lowByte = buffer[0];
            byte highByte = buffer[1];

            BtnA = (lowByte & 0x80) != 0;
            BtnX = (lowByte & 0x40) != 0;
            BtnL = (lowByte & 0x20) != 0;
            BtnR = (lowByte & 0x10) != 0;

            BtnB = (highByte & 0x80) != 0;
            BtnY = (highByte & 0x40) != 0;
            BtnSelect = (highByte & 0x20) != 0;
            BtnStart = (highByte & 0x10) != 0;
            DUp = (highByte & 0x08) != 0;
            DDown = (highByte & 0x04) != 0;
            DLeft = (highByte & 0x02) != 0;
            DRight = (highByte & 0x01) != 0;
        }

        public override void RefreshDisplay()
        {
            //nothing
        }
    }
}