using System.Net;

namespace MalignEngine.Network;

public enum PacketChannel
{
    Reliable,
    Unreliable
}

public interface ITransport : ServerTransport, ClientTransport
{
    public void Update();
}

public interface ServerTransport
{
    public event Action<NetworkConnection, IReadMessage> OnClientData;
    public event Action<NetworkConnection> OnClientConnected;
    public event Action<NetworkConnection> OnClientDisconnected;

    public void SendDataToClient(NetworkConnection connection, IWriteMessage message, PacketChannel channel);
    public void Disconnect(NetworkConnection connection);
    public void Listen(IPEndPoint endpoint);
    public void Stop();
}

public interface ClientTransport
{
    public event Action<IReadMessage> OnData;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public void SendDataToServer(IWriteMessage message, PacketChannel channel);
    public void Disconnect();
    public void TryConnect(IPEndPoint endpoint);
}