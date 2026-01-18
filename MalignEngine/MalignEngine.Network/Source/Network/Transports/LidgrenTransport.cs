using System.Net;
using Lidgren.Network;
using Microsoft.Extensions.Logging;

namespace MalignEngine.Network;

public partial class LidgrenTransport : ITransport
{
    private ILogger logger;

    private NetClient client;
    private NetServer server;

    public event Action<IReadMessage> OnData;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public event Action<NetworkConnection, IReadMessage> OnClientData;
    public event Action<NetworkConnection> OnClientConnected;
    public event Action<NetworkConnection> OnClientDisconnected;

    private Dictionary<long, NetworkConnection> connections = new Dictionary<long, NetworkConnection>();

    public LidgrenTransport(ILogger networkLogger)
    {
        logger = networkLogger;
    }

    public void SendDataToServer(IWriteMessage message, PacketChannel packetChannel = PacketChannel.Reliable)
    {
        if (client == null)
        {
            throw new ArgumentException("Tried to send data but the client is not connected to a server.");
        }

        NetOutgoingMessage msg = client.CreateMessage();
        msg.Write(message.Buffer);

        switch (packetChannel)
        {
            case PacketChannel.Reliable:
                client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
                break;
            case PacketChannel.Unreliable:
                client.SendMessage(msg, NetDeliveryMethod.Unreliable);
                break;
            default:
                throw new Exception("Unknown PacketChannel");
        }
    }

    public void SendDataToClient(NetworkConnection connection, IWriteMessage message, PacketChannel channel = PacketChannel.Reliable)
    {
        if (server == null)
        {
            throw new ArgumentException("Tried to send data but the server is not started.");
        }

        NetOutgoingMessage msg = server.CreateMessage();
        msg.Write(message.Buffer);

        NetConnection netConnection = null;
        foreach ((long remoteId, NetworkConnection c) in connections)
        {
            if (c.Id == connection.Id)
            {
                netConnection = server.Connections.First(x => x.RemoteUniqueIdentifier == remoteId);
                break;
            }
        }

        if (netConnection == null || !connection.IsValid)
        {
            throw new ArgumentException("Tried to send data to an invalid connection.");
        }

        switch (channel)
        {
            case PacketChannel.Reliable:
                server.SendMessage(msg, netConnection, NetDeliveryMethod.ReliableOrdered);
                break;
            case PacketChannel.Unreliable:
                server.SendMessage(msg, netConnection, NetDeliveryMethod.Unreliable);
                break;
            default:
                throw new Exception("Unknown PacketChannel");
        }
    }

    public void TryConnect(IPEndPoint endpoint)
    {
        NetPeerConfiguration config = new NetPeerConfiguration("MalignEngine");

        client = new NetClient(config);
        client.Start();
        client.Connect(endpoint);
    }

    public void Disconnect()
    {
        if (client == null) { return; }

        client.Disconnect("disconnect");
    }

    private void UpdateClient()
    {
        NetIncomingMessage msg;
        while ((msg = client.ReadMessage()) != null)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.WarningMessage:
                    logger.LogVerbose($"[client] {msg.SenderEndPoint}: {msg.ReadString()}");
                    break;
                case NetIncomingMessageType.ErrorMessage:
                    logger.LogError($"[client] {msg.SenderEndPoint}: {msg.ReadString()}");
                    break;
                case NetIncomingMessageType.StatusChanged:
                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                    logger.LogVerbose($"[client] {msg.SenderEndPoint}: Status Changed to {status}");

                    if (status == NetConnectionStatus.Connected)
                    {
                        OnConnected();
                    }

                    if (status == NetConnectionStatus.Disconnected)
                    {
                        OnDisconnected();
                    }

                    break;
                case NetIncomingMessageType.Data:
                    byte[] data = msg.ReadBytes(msg.LengthBytes);
                    IReadMessage readMessage = new ReadOnlyMessage(data, false, 0, data.Length);
                    OnData(readMessage);
                    break;
                default:
                    logger.LogWarning("[client] Unhandled type: " + msg.MessageType);
                    break;
            }
            client.Recycle(msg);
        }
    }

    private void UpdateServer()
    {
        NetIncomingMessage msg;
        while ((msg = server.ReadMessage()) != null)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.WarningMessage:
                    logger.LogVerbose($"[server] {msg.SenderEndPoint}: {msg.ReadString()}");
                    break;
                case NetIncomingMessageType.ErrorMessage:
                    logger.LogError($"[server] {msg.SenderEndPoint}: {msg.ReadString()}");
                    break;
                case NetIncomingMessageType.StatusChanged:
                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                    logger.LogVerbose($"[server] {msg.SenderEndPoint}: Status Changed to {status}");

                    if (status == NetConnectionStatus.Connected)
                    {
                        if (!connections.ContainsKey(msg.SenderConnection.RemoteUniqueIdentifier))
                        {
                            connections.Add(msg.SenderConnection.RemoteUniqueIdentifier, new NetworkConnection(msg.SenderConnection.RemoteUniqueIdentifier));
                        }

                        OnClientConnected(connections[msg.SenderConnection.RemoteUniqueIdentifier]);
                    }

                    if (status == NetConnectionStatus.Disconnected)
                    {
                        Disconnect(connections[msg.SenderConnection.RemoteUniqueIdentifier]);
                    }

                    break;
                case NetIncomingMessageType.Data:
                    if (!connections.ContainsKey(msg.SenderConnection.RemoteUniqueIdentifier))
                    {
                        return;
                    }

                    byte[] data = msg.ReadBytes(msg.LengthBytes);
                    IReadMessage readMessage = new ReadOnlyMessage(data, false, 0, data.Length);
                    readMessage.Sender = connections[msg.SenderConnection.RemoteUniqueIdentifier];
                    OnClientData(readMessage.Sender, readMessage);

                    break;
                default:
                    logger.LogWarning("[server] Unhandled type: " + msg.MessageType);
                    break;
            }
            server.Recycle(msg);
        }
    }

    public void Update()
    {
        if (client != null) { UpdateClient(); }
        if (server != null) { UpdateServer(); }
    }

    public void Disconnect(NetworkConnection connection)
    {
        if (server == null) { return; }

        NetConnection? netConnection = null;
        foreach ((long remoteId, NetworkConnection c) in connections)
        {
            if (c.Id == connection.Id)
            {
                netConnection = server.Connections.First(x => x.RemoteUniqueIdentifier == remoteId);
                break;
            }
        }

        if (netConnection == null || !connection.IsValid)
        {
            throw new ArgumentException("Tried to disconnect invalid connection.");
        }

        connection.Invalidate();

        netConnection.Disconnect("");
    }

    public void Listen(IPEndPoint endpoint)
    {
        NetPeerConfiguration config = new NetPeerConfiguration("MalignEngine")
        {
            LocalAddress = endpoint.Address,
            Port = endpoint.Port,
        };

        server = new NetServer(config);
        server.Start();
    }

    public void Stop()
    {
        server?.Shutdown(null);
    }
}
