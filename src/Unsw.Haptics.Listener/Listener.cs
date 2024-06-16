using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Unsw.Haptics.Common;

namespace Unsw.Haptics.Listener;

public class Listener : IHostedService // Interface from .Hosting (StartAsync, StopAsync)
{
    private readonly ILogger<Listener> _logger; // logger object for logging info, warnings, errors
    private readonly NetManager _netManager; // manage network connections and data transmission
    private Timer? _pollTimer; // timer for polling events
    private readonly NetPacketProcessor _netPacketProcessor = new(); // process incoming packets
    private readonly ListenerOptions _options; // store listener option (port and poll interval)

    public Listener(IEnumerable<IPacketHandler> packetHandlers, IOptions<ListenerOptions> options,
        ILogger<Listener> logger)
    {
        _logger = logger;

        var listener = new EventBasedNetListener(); // use as listener for _netManager

        _netManager = new NetManager(listener)
        {
            BroadcastReceiveEnabled = true,
            IPv6Enabled = IPv6Mode.Disabled
        };

        SetupListener(listener);

        _options = options.Value;

        /// <summary>
        /// Handle incoming HapticPackets from Netpeer. Call HandleAsync to handle each packet and wait for completion.
        /// OnPacketReceived subscribe to a  _netPacketProcessor, also specify packet type and source type
        /// </summary>
        async void OnPacketReceived(HapticPacket packet, NetPeer peer)
        {
            //_logger.LogInformation("Process Packet");
            var tasks = packetHandlers.Select(p => p.HandleAsync(packet));
           
            await Task.WhenAll(tasks);
        }

        _netPacketProcessor.SubscribeReusable<HapticPacket, NetPeer>(OnPacketReceived);
    }

    /// <summary>
    /// Start _netManager on given port and setup a timer for pollEvents at given interval
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _netManager.Start(_options.Port);
        _logger.LogInformation("net manager {info}", _netManager.Start(9050));
        _pollTimer = new Timer(_ => _netManager.PollEvents(), null, TimeSpan.Zero, _options.PollingInterval);

        _logger.LogInformation("Listening on *:{Port}", _options.Port);

        return Task.CompletedTask; 
    }

    /// <summary>
    /// Stop pollEvents and _netmanager
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _pollTimer?.Dispose();
        _netManager.Stop();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Setup event handlers for listeners
    /// </summary>
    private void SetupListener(EventBasedNetListener listener)
    {
        listener.ConnectionRequestEvent += request =>
            request.Accept(); // Automatically accept new client connecting to server

        listener.PeerConnectedEvent += peer =>
            _logger.LogInformation("Peer {Peer} connected", peer.EndPoint);

        listener.PeerDisconnectedEvent += (peer, info) =>
            _logger.LogInformation("Peer {Peer} disconnected {@DisconnectInfo}", peer.EndPoint, info);

        listener.NetworkErrorEvent += (endPoint, error) =>
            _logger.LogError("A network error: {Error} occured on {EndPoint}", endPoint, error);

        listener.NetworkReceiveEvent += (peer, reader, _) =>
        {
            _logger.LogInformation("Received packet from peer");
            _netPacketProcessor.ReadAllPackets(reader, peer); // Process received packet
        };
         

        listener.NetworkReceiveUnconnectedEvent += (endPoint, reader, type) =>
        {
            // Sends a response message to an unconnected client when server receives a message from unknown client
            _netManager.SendUnconnectedMessage(NetDataWriter.FromString("DISCOVERY RESPONSE"), endPoint);
        };

        listener.NetworkLatencyUpdateEvent += (peer, latency) =>
            _logger.LogInformation("Peer {Peer} latency: {Latency}ms", peer.EndPoint, latency);
    }
}