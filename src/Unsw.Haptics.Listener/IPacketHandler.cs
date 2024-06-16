using Unsw.Haptics.Common;

namespace Unsw.Haptics.Listener;

public interface IPacketHandler
{
    Task HandleAsync(HapticPacket packet);
}