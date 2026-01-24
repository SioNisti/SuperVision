using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVision;

public interface IWidget
{
    public virtual string DisplayName => GetType().Name.Replace("ViewModel", "");
    string WidgetType { get; }
    Dictionary<uint, uint> GetRequiredAddresses();
    void UpdateState(Dictionary<uint, byte[]> memoryData);
}