using MalignEngine;
using System.Net;
using System.Numerics;

namespace MalignEngine.Network;

public interface INetSerializable { }

public abstract class NetMessage
{
    public string MsgName { get; }
    protected NetMessage()
    {
        MsgName = GetType().Name;
    }

    public abstract void Deserialize(IReadMessage message);
    public abstract void Serialize(IWriteMessage message);
}

public class ClientDataNetMessage : NetMessage
{
    public byte ClientId;

    public override void Deserialize(IReadMessage message)
    {
        ClientId = message.ReadByte();
    }

    public override void Serialize(IWriteMessage message)
    {
        message.WriteByte(ClientId);
    }
}

public interface INetworkingService : IService
{
    public void SetTransport(ITransport transport);
    public void RegisterNetMessage<T>(Action<T>? callback = null) where T : NetMessage;
    public void StartServer(IPEndPoint endpoint);
    public void StopServer();
    /// <summary>
    /// Tries to connect the client to the specified endpoint
    /// </summary>
    /// <param name="endpoint"></param>
    public void TryConnect(IPEndPoint endpoint);
    /// <summary>
    /// Disconnects a connection from the server
    /// </summary>
    public void Disconnect(NetworkConnection connection);
    /// <summary>
    /// Disconnects the client connection from the server
    /// </summary>
    public void Disconnect();
    /// <summary>
    /// Sends a net message from the server to a client
    /// </summary>
    public void SendNetMessage<T>(T message, NetworkConnection connection) where T : NetMessage;
    /// <summary>
    /// Sends a net message from the client to the server
    /// </summary>
    public void SendNetMessage<T>(T message) where T : NetMessage;
}

public class ServerStartEvent : EventArgs { }
public class ServerStopEvent : EventArgs { }
public class ClientConnected : EventArgs
{
    public NetworkConnection Connection;
}
public class ClientDisconnected : EventArgs
{
    public NetworkConnection Connection;
}
public class Connected : EventArgs { }
public class Disconnected : EventArgs { }

public partial class NetworkingService : BaseSystem, INetworkingService
{
    private class MessageData
    {
        public Type Type;
        public Action<NetMessage>? Callback;
    }

    public bool IsServerRunning { get; private set; }
    public bool IsClientRunning { get; private set; }

    [Dependency]
    protected EventService EventService = default!;

    private ILogger logger;
    private NetSerializer.Serializer serializer;
    private ITransport transport;
    private Dictionary<string, MessageData> netReceives;

    public NetworkingService()
    {
        logger = LoggerService.GetSawmill("networking");

        netReceives = new Dictionary<string, MessageData>();

        serializer = new NetSerializer.Serializer(new List<Type>() { typeof(INetSerializable) });

        RegisterNetMessage<ClientDataNetMessage>(ClientDataReceived);
    }

    private void ClientDataReceived(ClientDataNetMessage message)
    {
        if (!IsClientRunning) { return; }
    }

    public void SetTransport(ITransport transport)
    {
        transport.OnClientConnected += OnClientConnected;
        transport.OnClientDisconnected += OnClientDisconnected;

        transport.OnConnected += OnConnected;
        transport.OnDisconnected += OnDisconnected;

        transport.OnClientData += OnClientData;
        transport.OnData += OnData;
    }

    private void OnData(IReadMessage message)
    {
        string msgName = message.ReadString();
        if (!netReceives.TryGetValue(msgName, out MessageData messageData))
        {
            logger.LogWarning($"Received unknown message: {msgName}");
        }

        NetMessage msg = (NetMessage)Activator.CreateInstance(messageData.Type);
        msg.Deserialize(message);

        messageData.Callback?.Invoke(msg);
    }

    private void OnClientData(NetworkConnection connection, IReadMessage message) => OnData(message);

    public void StartServer(IPEndPoint endpoint)
    {
        transport.Listen(endpoint);

        IsServerRunning = true;
    }

    public void StopServer()
    {
        transport.Stop();

        IsServerRunning = false;
    }

    public void TryConnect(IPEndPoint endpoint)
    {
        transport.TryConnect(endpoint);
    }

    public void Disconnect()
    {
        transport.Disconnect();
    }

    public void Disconnect(NetworkConnection connection)
    {
        transport.Disconnect(connection);
    }

    public void RegisterNetMessage<T>(Action<T>? callback = null) where T : NetMessage
    {
        netReceives.Add(typeof(T).Name, new MessageData() { Type = typeof(T), Callback = (NetMessage message) => callback?.Invoke((T)message) });
    }

    public void SendNetMessage<T>(T message) where T : NetMessage
    {
        if (!netReceives.TryGetValue(message.MsgName, out MessageData messageData))
        {
            throw new ArgumentException("Tried to send message of a type that is not registered");
        }

        IWriteMessage writeMessage = new WriteOnlyMessage();
        writeMessage.WriteString(message.MsgName); // very bad, optimize later
        message.Serialize(writeMessage);

        transport.SendDataToServer(writeMessage, PacketChannel.Reliable);
    }

    public void SendNetMessage<T>(T message, NetworkConnection connection) where T : NetMessage
    {
        if (!netReceives.TryGetValue(message.MsgName, out MessageData messageData))
        {
            throw new ArgumentException("Tried to send message of a type that is not registered");
        }

        IWriteMessage writeMessage = new WriteOnlyMessage();
        writeMessage.WriteString(message.MsgName); // very bad, optimize later
        message.Serialize(writeMessage);
        
        transport.SendDataToClient(connection, writeMessage, PacketChannel.Reliable);
    }

    public override void OnUpdate(float deltaTime)
    {
        transport.Update();
    }

    private void OnDisconnected()
    {
        IsClientRunning = false;
        EventService.Get<EventChannel<Disconnected>>().Raise(new Disconnected());
    }

    private void OnConnected()
    {
        IsClientRunning = false;
        EventService.Get<EventChannel<Connected>>().Raise(new Connected());
    }

    private void OnClientDisconnected(NetworkConnection connection)
    {
        EventService.Get<EventChannel<ClientDisconnected>>().Raise(new ClientDisconnected { Connection = connection });
    }

    private void OnClientConnected(NetworkConnection connection)
    {
        EventService.Get<EventChannel<ClientConnected>>().Raise(new ClientConnected { Connection = connection });
    }

}