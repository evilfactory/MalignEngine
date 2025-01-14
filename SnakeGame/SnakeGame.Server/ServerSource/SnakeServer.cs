using MalignEngine;

namespace SnakeGame;

public class SnakeServer : EntitySystem
{
    public Transport Transport { get; private set; }

    private List<NetworkConnection> clients = new List<NetworkConnection>();

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

        logger.LogInfo($"Listening on port {port}");
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

        clients.Add(connection);
    }

    public void OnClientDisconnected(NetworkConnection connection, DisconnectReason reason)
    {
        logger.LogInfo($"Client disconnected: {connection.Id}");

        clients.Remove(connection);
    }

    public void OnMessageReceived(IReadMessage message)
    {
        logger.LogInfo($"Message received from {message.Sender.Id}");

        byte clientTick = message.ReadByte();
        byte messageCount = message.ReadByte();

        for (int i = 0; i < messageCount; i++)
        {
            ClientToServer messageType = (ClientToServer)message.ReadByte();

            switch (messageType)
            {
                case ClientToServer.RequestLobbyInfo:
                    RequestLobbyInfo(message);
                    break;
            }
        }
    }

    public void SendMessageToClient(NetworkConnection connection, ServerToClient messageType, IWriteMessage message)
    {
        message.WriteByte(0);
        message.WriteByte(1);
        message.WriteByte((byte)messageType);
        SendToClient(message, connection);
    }

    private void RequestLobbyInfo(IReadMessage message)
    {
        WriteOnlyMessage response = new WriteOnlyMessage();

        response.WriteByte((byte)clients.Count);
        response.WriteString("拼图是毛茸茸的");
        response.WriteString("这是真的");
        response.WriteByte(0);
        response.WriteByte(0);
        response.WriteString("Evil Snake Server");

        SendMessageToClient(message.Sender, ServerToClient.LobbyInformation, response);
    }
}
