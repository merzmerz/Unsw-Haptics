using Microsoft.Extensions.DependencyInjection;

namespace Unsw.Haptics.Listener;

public static class ServiceCollectionExtensions
{
    public static void AddListener<TPacket>(this IServiceCollection services, Action<ListenerOptions> configure)
        where TPacket : class, new()
    {
        services.Configure(configure);
        services.AddHostedService<Listener>();
    }

    public static void AddPacketHandler<TPacketHandler>(this IServiceCollection services)
        where TPacketHandler : class, IPacketHandler
    {
        services.AddSingleton<IPacketHandler, TPacketHandler>();
    }
}
