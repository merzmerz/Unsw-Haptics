namespace Unsw.Haptics.Zaber.App;

public class ZaberControllerOptions 
{
    public string? PortName { get; set; }
    public Dictionary<int, ZaberMovementRange>? MovementRanges { get; set; }
}