using Unsw.Haptics.Common;

namespace Unsw.Haptics.Zaber.App.Configuration;

public class ZaberConfiguration
{
    public int DeviceId { get; set; }
    public double MinPosition { get; set; }
    public double MaxPosition { get; set; }
    public Handedness Handedness { get; set; }
    public Finger Finger { get; set; }
}