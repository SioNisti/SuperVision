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
            { 0xF510C4, 2 } //this and the next address hold the inputs in binary
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

        public override void UpdateState(Dictionary<uint, byte[]> data)
        {
            if (!data.TryGetValue(0xF510C4, out var buffer) || buffer.Length < 2) return;

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