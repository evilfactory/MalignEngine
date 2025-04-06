using System.Net;

namespace MalignEngine;

public interface ITransport { }

public interface ServerTransport : ITransport
{
    public event Action<NetworkConnection> OnClientConnected;
    public event Action<NetworkConnection> OnClientDisconnected;
    public void SendData(NetworkConnection connection, IWriteMessage message);
    public void Disconnect(NetworkConnection connection);
    public void Listen(IPEndPoint endpoint);
    public void Stop();
}

public interface ClientTransport : ITransport
{
    public event Action OnConnected;
    public event Action OnDisconnected;

    public void SendData(IWriteMessage message);
    public void Disconnect();
    public void TryConnect(IPEndPoint endpoint);
}