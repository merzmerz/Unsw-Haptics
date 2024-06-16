using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Unsw.Haptics.Common;
using Unsw.Haptics.Listener;
using Unsw.Haptics.Zaber.App;
using Unsw.Haptics.Zaber.App.Configuration;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging(o =>
        {
            o.AddSimpleConsole(c =>
            {
                c.SingleLine = true;
                c.TimestampFormat = "[dd-MM-yyyy HH:mm:ss] ";
            });
        });
        // get config value from appsettings.json into AppConfiguration
        var config = hostContext.Configuration.Get<AppConfiguration>();

        services.AddSingleton<ZaberController>(); // regis ZaberController as singleton instance
        services.Configure<ZaberControllerOptions>(o =>
        {
            o.PortName = "d44e80ed-b59d-4002-a2a1-5a12690562de";
            o.MovementRanges = config.Zaber.ToDictionary(z => z.DeviceId, z => new ZaberMovementRange
            {
                Minimum = z.MinPosition,
                Maximum = z.MaxPosition
            });
        });

        // add listener service that listen for HapticPacket
        services.AddListener<HapticPacket>(options =>
        {
            options.Port = 9050;
            options.PollingInterval = TimeSpan.FromMilliseconds(190);
        });

        // add ZaberPacketHandler Class as packethandler service
        services.AddPacketHandler<ZaberPacketHandler>();

        services.Configure<ZaberPacketHandlerOptions>(o =>
        {
            o.LabelTensionMappings = config.Tensions;
            o.ZaberDevices = config.Zaber.ToDictionary(z => (z.Handedness, z.Finger), z => z.DeviceId);
        });

    })
    .Build();

host.Run();