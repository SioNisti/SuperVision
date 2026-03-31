using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperVision
{
    public class LayoutSaveData
    {
        public double WindowWidth { get; set; } = 200;
        public double WindowHeight { get; set; } = 200;
        /*public string FontName { get; set; } = "Courier New, Monospace, Consolas";
        public int FontSize { get; set; } = 22;
        [JsonConverter(typeof(ColorJsonConverter))] public Color FontColor { get; set; } = Colors.White;
        [JsonConverter(typeof(ColorJsonConverter))] public Color BgColor { get; set; } = Colors.Black;*/
        public List<WidgetSettings> Widgets { get; set; } = new();
    }
    public partial class WidgetSettings : ObservableObject
    {
        public string Type { get; set; } = "Splits";
        public string FontName { get; set; } = "Courier New, Monospace, Consolas";
        public int FontSize { get; set; } = 22;
        [JsonConverter(typeof(ColorJsonConverter))] public Color FontColor { get; set; } = Colors.White;
        [JsonConverter(typeof(ColorJsonConverter))] public Color BgColor { get; set; } = Colors.Black;
        public Dictionary<string, string> Variables { get; set; } = new();
        //public bool GlobalStyle { get; set; } = false;
    }
}