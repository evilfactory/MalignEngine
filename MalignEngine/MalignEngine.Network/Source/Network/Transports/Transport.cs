using System.Net;

namespace MalignEngine.Network;

public enum PacketChannel
{
    Reliable,
    Unreliable
}

public interface ITransport { }

public interface IClientTransport : ITransport
{
    event Action Connected;
    event Action Disconnected;
    event Action<IReadMessage> Received;

    void Connect(IPEndPoint endpoint);
    void Disconnect();
    void Send(IWriteMessage payload, PacketChannel channel);
    void Update();
}

public interface IServerTransport : ITransport
{
    IEnumerable<NetworkConnection> Connections { get; }

    event Action<NetworkConnection> ClientConnected;
    event Action<NetworkConnection> ClientDisconnected;
    event Action<NetworkConnection, IReadMessage> Received;

    void Start();
    void Stop();
    void Send(NetworkConnection connection, IWriteMessage payload, PacketChannel channel);
    void Disconnect(NetworkConnection connection);
    void Update();
}