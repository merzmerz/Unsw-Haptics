namespace Unsw.Haptics.Common;

public class HapticPacket
{
    public HapticEvent HapticEvent { get; set; }
    public Handedness Handedness { get; set; }
    public Finger Finger { get; set; }
    public string? Label { get; set; }
    public float Depth { get; set; }

    public override string ToString() => string.Join(' ', Handedness, Finger, HapticEvent, Label);
}