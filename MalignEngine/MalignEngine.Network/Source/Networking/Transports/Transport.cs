using System.Net;

namespace MalignEngine.Network;

public enum PacketChannel
{
    Reliable,
    Unreliable
}

public interface ITransport
{
    public void Update();
}

public interface ServerTransport : ITransport
{
    public event Action<NetworkConnection, IReadMessage> OnData;
    public event Action<NetworkConnection> OnClientConnected;
    public event Action<NetworkConnection> OnClientDisconnected;

    public void SendData(NetworkConnection connection, IWriteMessage message, PacketChannel channel);
    public void Disconnect(NetworkConnection connection);
    public void Listen(IPEndPoint endpoint);
    public void Stop();
}

public interface ClientTransport : ITransport
{
    public event Action<IReadMessage> OnClientData;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public void SendData(IWriteMessage message, PacketChannel channel);
    public void Disconnect();
    public void TryConnect(IPEndPoint endpoint);
}