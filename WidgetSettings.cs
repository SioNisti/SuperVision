using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SuperVision
{
    public partial class WidgetSettings : ObservableObject
    {
        public string Type { get; set; } = "Splits";
        public string FontColor { get; set; } = "#FFFFFF";
        public string BgColor { get; set; } = "#00000000";

        [ObservableProperty]
        [JsonIgnore]
        private string _displayText = ""; 
    }
}