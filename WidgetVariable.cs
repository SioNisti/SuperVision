using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SuperVision
{
    // Defines a single configurable variable
    public partial class WidgetVariable : ObservableObject
    {
        public string Name { get; set; }
        public string VarType { get; set; }
        public List<string> Options { get; set; } = new();

        [ObservableProperty]
        private string _value;

        public WidgetVariable(string name, string type, string defaultValue, List<string>? options = null)
        {
            Name = name;
            VarType = type;
            Value = defaultValue;
            if (options != null) Options = options;
        }
    }
}