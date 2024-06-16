using Microsoft.Extensions.Options;
using Unsw.Haptics.Common;
using Zaber.Motion;
using Zaber.Motion.Ascii;
//ing Zaber.Motion.Binary;

namespace Unsw.Haptics.Zaber.App;

public class ZaberController : IDisposable
{
    private readonly Connection _connection; // manage communication between computer and zaber
    private readonly IDictionary<int, ZaberDevice> _devices;

    public ZaberController(IOptions<ZaberControllerOptions> optionsProvider)
    {
        var options = optionsProvider.Value; // access zaber controller options(port and movementrange)
        Library.EnableDeviceDbStore(); // enable database storage
        //_connection = Connection.OpenSerialPort(options.PortName); // For initializing connection with physical Zaber Linear stage device
        _connection = Connection.OpenIot(options.PortName); // For initializing connection with virtual Zaber Linear stage device
        Console.WriteLine($"Port {options.PortName} .");
        _connection.EnableAlerts();
        var deviceList = _connection.DetectDevices();
        Console.WriteLine($"Found {deviceList.Length} devices.");
        var device = deviceList[0];

        var axis = device.GetAxis(1);
        if (!axis.IsHomed())
        {
            axis.Home();
        }

        _devices = options.MovementRanges.ToDictionary(r => r.Key, r =>
        {
            var axis = device.GetAxis(1);
            return new ZaberDevice(axis, r.Value);
        });
        
    }

    /// <summary>
    /// Change the tension
    /// </summary>
    /// <param name="deviceId">The ID of the Zaber device</param>
    /// <param name="tension">A tension value within the range [0,1] </param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Task ChangeTensionAsync(int deviceId, double tension)
    {
        if (tension is < 0 or > 1)
            tension = 1;
            //throw new ArgumentOutOfRangeException(nameof(tension));

        // break once
        if (!_devices.TryGetValue(deviceId, out var device))
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }
            
        Console.WriteLine("Tension in controller: " + tension);
        device.TensionHistory.Push(tension);
        return SetTensionAsync(device, tension);
    }

    /// <summary>
    /// Change the tension to what it was previously
    /// </summary>
    /// <param name="deviceId">The ID of the Zaber device</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task RevertTensionAsync(int deviceId)
    {
        if (!_devices.TryGetValue(deviceId, out var device))
            throw new ArgumentOutOfRangeException(nameof(deviceId));

        // Pop two tensions off the stack falling back to 0; if empty or have one value
        if (!device.TensionHistory.TryPop(out _) || !device.TensionHistory.TryPop(out var tension))
            tension = 0;

        await SetTensionAsync(device, tension);
    }

    private static async Task SetTensionAsync(ZaberDevice device, double tension)
    {
        try
        {
            var position = device.MovementRange.Maximum -
                           tension * (device.MovementRange.Maximum - device.MovementRange.Minimum);
            Console.WriteLine("Position: " + position);

            if (position >= device.MovementRange.Minimum)
            {
                await device.Axis.MoveAbsoluteAsync(position, Units.Length_Millimetres);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error" + e);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private class ZaberDevice
    {
        public ZaberDevice(Axis axis, ZaberMovementRange movementRange)
        {
            Axis = axis;
            MovementRange = movementRange;
            TensionHistory = new Stack<double>();
        }

        public Axis Axis { get; }
        public ZaberMovementRange MovementRange { get; }
        public Stack<double> TensionHistory { get; }
    }
}
