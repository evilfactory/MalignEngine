using MalignEngine;

namespace SnakeGame;

public class SnakeServer : EntitySystem
{
    public Transport Transport { get; private set; }

    protected ILogger logger;

    public override void OnInitialize()
    {
        logger = LoggerService.GetSawmill("snake-server");

        Transport = new TcpTransport();
        Transport.Logger = logger;

        Transport.OnClientConnected = OnClientConnected;
        Transport.OnClientDisconnected = OnClientDisconnected;
        Transport.OnMessageReceived = OnMessageReceived;
    }

    public void Listen(int port)
    {
        Transport.Listen(port);
    }

    public void Shutdown()
    {
        Transport.Shutdown();
    }

    public void SendToClient(IWriteMessage message, NetworkConnection connection, PacketChannel packetChannel = PacketChannel.Reliable)
    {
        Transport.SendToClient(message, connection, packetChannel);
    }

    public void OnClientConnected(NetworkConnection connection)
    {
        logger.LogInfo($"Client connected: {connection.Id}");
    }

    public void OnClientDisconnected(NetworkConnection connection)
    {
        logger.LogInfo($"Client disconnected: {connection.Id}");
    }

    public void OnMessageReceived(IReadMessage message)
    {
        logger.LogInfo($"Message received from {message.Sender.Id}");
    }
}
