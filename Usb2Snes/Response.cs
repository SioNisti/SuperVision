using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperVision.Usb2Snes
{
    public class Response
    {
        [JsonPropertyName("Results")]
        public List<string> Results { get; set; } = new List<string>();
    }
}