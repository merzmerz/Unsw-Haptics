using Microsoft.Extensions.Options;
using Unsw.Haptics.Common;
using Unsw.Haptics.Listener;

namespace Unsw.Haptics.Zaber.App
{
    public class ZaberPacketHandler : IPacketHandler
    {
        private readonly ZaberPacketHandlerOptions _options;
        private readonly ZaberController _zaberController;
        private float InitialTension = 0.2f;

        public ZaberPacketHandler(IOptions<ZaberPacketHandlerOptions> options, ZaberController zaberController)
        {
            _options = options.Value;
            _zaberController = zaberController;
        }

        public async Task HandleAsync(HapticPacket hapticPacket)
        {
            Console.WriteLine("Packet: " + hapticPacket);
            Console.WriteLine("depth: " + hapticPacket.Depth);
            var zaberDeviceId = GetZaberDeviceId(hapticPacket);
            Console.WriteLine("zaberId: " + zaberDeviceId);
            if (!zaberDeviceId.HasValue)
                return;

            if (hapticPacket.HapticEvent == HapticEvent.EnterObject)
            {
                var tension = InitialTension; // initial tension, 1% of maximum tension (145-115)*0.1
                Console.WriteLine("tension: " + tension);
                await _zaberController.ChangeTensionAsync(zaberDeviceId.Value, tension);
            }
            else if (hapticPacket.HapticEvent == HapticEvent.PressObject)
            {
                var tension = InitialTension + hapticPacket.Depth * GetTension(hapticPacket); // apply more tension when push
                Console.WriteLine("tension: " + tension);
                await _zaberController.ChangeTensionAsync(zaberDeviceId.Value, tension);
            }
            else
            {
                await _zaberController.ChangeTensionAsync(zaberDeviceId.Value, 0);
                //await _zaberController.RevertTensionAsync(zaberDeviceId.Value);
            }
        }

        private int? GetZaberDeviceId(HapticPacket packet)
        {
            // Check ZaberDevices dict contains a key matching Hand and Finger, if found return corresponding value deviceId
            if (_options.ZaberDevices.TryGetValue((packet.Handedness, packet.Finger), out var deviceId))
                return deviceId;

            // return null
            return default;
        }

        private double GetTension(HapticPacket packet)
        {
            if (packet.Label != null && _options.LabelTensionMappings.TryGetValue(packet.Label, out var tension))
                return tension;

            return 0;
        }
    }
}
