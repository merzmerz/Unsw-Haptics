using Unsw.Haptics.Common;

namespace Unsw.Haptics.Zaber.App;

public class ZaberPacketHandlerOptions
{
    public IDictionary<string, double> LabelTensionMappings { get; set; }
    public IDictionary<(Handedness, Finger), int> ZaberDevices { get; set; }
}