using MalignEngine;
using System.Net;

namespace SnakeGame;

public class SnakeClient : EntitySystem
{
    public Transport Transport { get; private set; }

    protected ILogger logger;

    public override void OnInitialize()
    {
        logger = LoggerService.GetSawmill("snake-client");

        Transport = new TcpTransport();
        Transport.Logger = logger;

        Transport.OnConnected = OnConnected;
        Transport.OnDisconnected = OnDisconnected;
        Transport.OnMessageReceived = OnMessageReceived;
    }

    public void Connect(IPEndPoint endpoint)
    {
        Transport.Connect(endpoint);
    }

    public void Disconnect()
    {
        Transport.Disconnect(DisconnectReason.DisconnectedByUser);
    }

    public void SendToServer(IWriteMessage message, PacketChannel packetChannel = PacketChannel.Reliable)
    {
        Transport.SendToServer(message, packetChannel);
    }

    public void OnConnected()
    {
        logger.LogInfo("Connected to server");

        var message = new WriteOnlyMessage();
        message.WriteString("Hello, server!");

        Transport.SendToServer(message, PacketChannel.Reliable);
    }

    public void OnDisconnected(DisconnectReason reason)
    {
        logger.LogInfo("Disconnected from server");
    }

    public void OnMessageReceived(IReadMessage message)
    {
        logger.LogInfo($"Message received from server");

        var text = message.ReadString();

        logger.LogInfo($"Message: {text}");
    }
}
