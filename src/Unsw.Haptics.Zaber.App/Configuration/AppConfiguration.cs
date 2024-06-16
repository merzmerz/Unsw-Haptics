namespace Unsw.Haptics.Zaber.App.Configuration;

public class AppConfiguration
{
    public IEnumerable<ZaberConfiguration> Zaber { get; set; } = null!;
    public Dictionary<string, double> Tensions { get; set; } = null!;
}