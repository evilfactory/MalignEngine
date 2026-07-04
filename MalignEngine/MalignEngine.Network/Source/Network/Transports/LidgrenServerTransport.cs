using Lidgren.Network;

namespace MalignEngine.Network;

public sealed class LidgrenServerTransport : IServerTransport
{
    public event Action<NetworkConnection>? ClientConnected;
    public event Action<NetworkConnection>? ClientDisconnected;
    public event Action<NetworkConnection, IReadMessage>? Received;

    public IEnumerable<NetworkConnection> Connections => _connections.Values;

    private readonly NetServer _server;

    private readonly Dictionary<NetConnection, NetworkConnection> _connections = new();
    private readonly Dictionary<long, NetConnection> _lidgrenConnections = new();

    private long _nextConnectionId = 1;

    public LidgrenServerTransport(NetPeerConfiguration configuration)
    {
        _server = new NetServer(configuration);
    }

    public void Start()
    {
        _server.Start();
    }

    public void Stop()
    {
        _server.Shutdown("Server stopped");

        _connections.Clear();
        _lidgrenConnections.Clear();

        _nextConnectionId = 1;
    }

    public void Disconnect(NetworkConnection connection)
    {
        if (_lidgrenConnections.TryGetValue(connection.Id, out var lidgren))
        {
            lidgren.Disconnect("Disconnected");
        }
    }

    public void Send(NetworkConnection connection, IWriteMessage payload, PacketChannel channel)
    {
        if (!_lidgrenConnections.TryGetValue(connection.Id, out var lidgren))
            return;

        var msg = _server.CreateMessage();

        msg.Write(payload.Buffer);

        _server.SendMessage(
            msg,
            lidgren,
            channel == PacketChannel.Reliable
                ? NetDeliveryMethod.ReliableOrdered
                : NetDeliveryMethod.Unreliable);
    }

    public void Update()
    {
        NetIncomingMessage? msg;

        while ((msg = _server.ReadMessage()) != null)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.StatusChanged:
                    {
                        var status = (NetConnectionStatus)msg.ReadByte();

                        switch (status)
                        {
                            case NetConnectionStatus.Connected:
                                {
                                    var connection = new NetworkConnection(this, _nextConnectionId++);

                                    _connections[msg.SenderConnection] = connection;
                                    _lidgrenConnections[connection.Id] = msg.SenderConnection;

                                    ClientConnected?.Invoke(connection);
                                    break;
                                }

                            case NetConnectionStatus.Disconnected:
                                {
                                    if (_connections.TryGetValue(msg.SenderConnection, out var connection))
                                    {
                                        _connections.Remove(msg.SenderConnection);
                                        _lidgrenConnections.Remove(connection.Id);

                                        ClientDisconnected?.Invoke(connection);
                                    }

                                    break;
                                }
                        }

                        break;
                    }

                case NetIncomingMessageType.Data:
                    {
                        if (_connections.TryGetValue(msg.SenderConnection, out var connection))
                        {
                            var reader = new ReadOnlyMessage(msg.Data, false, msg.PositionInBytes, msg.LengthBytes);

                            Received?.Invoke(connection, reader);
                        }

                        break;
                    }
            }

            _server.Recycle(msg);
        }
    }
}