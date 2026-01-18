using System.Net;
using System.Runtime.CompilerServices;

namespace MalignEngine.Network;

public interface INetSerializable { }

public enum NetworkTarget
{
    Client,
    Server
}

public abstract class NetMessage
{
    public string MsgName { get; }
    public PacketChannel Channel => PacketChannel.Reliable;

    protected NetMessage()
    {
        MsgName = GetType().Name;
    }

    public abstract void Deserialize(IReadMessage message);
    public abstract void Serialize(IWriteMessage message);
}

public interface IServerStart : ISchedule
{
    void OnServerStarted();
}

public interface IServerStop : ISchedule
{
    void OnServerStopped();
}

public interface IClientConnectedToServer : ISchedule
{
    void OnClientConnectedToServer(NetworkConnection connection);
}

public interface IClientDisconnectedFromServer : ISchedule
{
    void OnClientDisconnectedFromServer(NetworkConnection connection);
}

public interface IConnectedToServer : ISchedule
{
    void OnConnectedToServer();
}

public interface IDisconnectedFromServer : ISchedule
{
    void OnDisconnectedFromServer();
}

public delegate void ReceiveServerNetMessageDelegate<T>(NetworkConnection connection, T netMessage) where T : NetMessage;
public delegate void ReceiveClientNetMessageDelegate<T>(T netMessage) where T : NetMessage;

public interface INetworkService
{
    /// <summary>
    /// Connections currently active on the server.
    /// </summary>
    IEnumerable<NetworkConnection> Connections { get; }
    bool IsServer { get; }
    bool IsClient { get; }

    void SetTransport(ITransport transport);
    void RegisterClientNetMessage<T>(ReceiveClientNetMessageDelegate<T>? callback = null) where T : NetMessage;
    void RegisterServerNetMessage<T>(ReceiveServerNetMessageDelegate<T>? callback = null) where T : NetMessage;
    void StartServer(IPEndPoint endpoint);
    void StopServer();
    /// <summary>
    /// Tries to connect the client to the specified endpoint
    /// </summary>
    /// <param name="endpoint"></param>
    void TryConnect(IPEndPoint endpoint);
    /// <summary>
    /// Disconnects a connection from the server
    /// </summary>
    void DisconnectConnection(NetworkConnection connection);
    /// <summary>
    /// Disconnects the client connection from the server
    /// </summary>
    void Disconnect();
    /// <summary>
    /// Sends a net message from the server to a client
    /// </summary>
    void SendToClient<T>(T message, NetworkConnection connection) where T : NetMessage;
    /// <summary>
    /// Sends a net message from the client to the server
    /// </summary>
    public void SendToServer<T>(T message) where T : NetMessage;
}

public partial class NetworkSystem : ISystem, INetworkService, IUpdate, IDisposable
{
    private record MessageData(Type Type, NetworkTarget Target, Action<NetworkConnection, NetMessage>? Callback);

    public bool IsServer { get; private set; }
    public bool IsClient { get; private set; }

    private readonly List<NetworkConnection> _connections;
    public IEnumerable<NetworkConnection> Connections => _connections;

    private readonly ILogger _logger;
    private readonly IScheduleManager _scheduleManager;

    private ITransport? _transport;
    private readonly Dictionary<string, MessageData> _netReceives;
    private readonly NetSerializer.Serializer _serializer;

    public NetworkSystem(ILoggerService loggerService, IScheduleManager scheduleManager)
    {
        _logger = loggerService.GetSawmill("networking");
        _scheduleManager = scheduleManager;

        _netReceives = new Dictionary<string, MessageData>();
        _connections = new List<NetworkConnection>();
        _serializer = new NetSerializer.Serializer(new List<Type>() { typeof(INetSerializable) });

        _scheduleManager.RegisterAll(this);
    }

    public void SetTransport(ITransport transport)
    {
        _transport = transport;

        _transport.OnClientConnected += OnClientConnected;
        _transport.OnClientDisconnected += OnClientDisconnected;

        _transport.OnConnected += OnConnected;
        _transport.OnDisconnected += OnDisconnected;

        _transport.OnClientData += OnClientData;
        _transport.OnData += OnData;
    }

    private void OnData(IReadMessage message)
    {
        string msgName = message.ReadString();
        if (!_netReceives.TryGetValue(msgName, out MessageData? messageData))
        {
            _logger.LogWarning($"Received unknown message: {msgName}");
            return;
        }

        if (!IsServer && messageData.Target == NetworkTarget.Server)
        {
            _logger.LogWarning($"Received message for wrong target: {msgName}");
            return;
        }

        if (!IsClient && messageData.Target == NetworkTarget.Client)
        {
            _logger.LogWarning($"Received message for wrong target: {msgName}");
            return;
        }

        NetMessage msg = (NetMessage)RuntimeHelpers.GetUninitializedObject(messageData.Type)!;
        msg.Deserialize(message);

        messageData.Callback?.Invoke(message.Sender!, msg);
    }

    private void OnClientData(NetworkConnection connection, IReadMessage message) => OnData(message);

    public void StartServer(IPEndPoint endpoint)
    {
        if (_transport == null)
        {
            throw new InvalidOperationException("transport was null");
        }

        _transport.Listen(endpoint);

        IsServer = true;
    }

    public void StopServer()
    {
        if (_transport == null)
        {
            throw new InvalidOperationException("transport was null");
        }

        _connections.Clear();

        _transport.Stop();

        IsServer = false;
    }

    public void TryConnect(IPEndPoint endpoint)
    {
        if (_transport == null)
        {
            throw new InvalidOperationException("transport was null");
        }

        _transport.TryConnect(endpoint);
    }

    public void Disconnect()
    {
        if (_transport == null)
        {
            throw new InvalidOperationException("transport was null");
        }

        _transport.Disconnect();
    }

    public void DisconnectConnection(NetworkConnection connection)
    {
        if (_transport == null)
        {
            throw new InvalidOperationException("transport was null");
        }

        _transport.Disconnect(connection);
    }

    public void RegisterServerNetMessage<T>(ReceiveServerNetMessageDelegate<T>? callback = null) where T : NetMessage
    {
        _netReceives.Add(typeof(T).Name,
            new MessageData(typeof(T), NetworkTarget.Server, (NetworkConnection connection, NetMessage message) => callback?.Invoke(connection, (T)message)));
    }

    public void RegisterClientNetMessage<T>(ReceiveClientNetMessageDelegate<T>? callback = null) where T : NetMessage
    {
        _netReceives.Add(typeof(T).Name,
            new MessageData(typeof(T), NetworkTarget.Client, (NetworkConnection connection, NetMessage message) => callback?.Invoke((T)message)));
    }

    public void SendToServer<T>(T message) where T : NetMessage
    {
        if (_transport == null)
        {
            throw new InvalidOperationException("transport was null");
        }

        if (!_netReceives.TryGetValue(message.MsgName, out MessageData? messageData))
        {
            throw new ArgumentException("Tried to send message of a type that is not registered");
        }

        IWriteMessage writeMessage = new WriteOnlyMessage();
        writeMessage.WriteString(message.MsgName); // very bad, optimize later
        message.Serialize(writeMessage);

        _transport.SendDataToServer(writeMessage, message.Channel);
    }

    public void SendToClient<T>(T message, NetworkConnection connection) where T : NetMessage
    {
        if (_transport == null)
        {
            throw new InvalidOperationException("transport was null");
        }

        if (!_netReceives.TryGetValue(message.MsgName, out MessageData? messageData))
        {
            throw new ArgumentException("Tried to send message of a type that is not registered");
        }

        IWriteMessage writeMessage = new WriteOnlyMessage();
        writeMessage.WriteString(message.MsgName); // very bad, optimize later
        message.Serialize(writeMessage);
        
        _transport.SendDataToClient(connection, writeMessage, message.Channel);
    }

    public void OnUpdate(float deltaTime)
    {
        _transport?.Update();
    }

    private void OnDisconnected()
    {
        _scheduleManager.Run<IDisconnectedFromServer>(x => x.OnDisconnectedFromServer());

        IsClient = false;
    }

    private void OnConnected()
    {
        IsClient = true;

        _scheduleManager.Run<IConnectedToServer>(x => x.OnConnectedToServer());
    }

    private void OnClientDisconnected(NetworkConnection connection)
    {
        _connections.Remove(connection);
        _scheduleManager.Run<IClientConnectedToServer>(x => x.OnClientConnectedToServer(connection));
    }

    private void OnClientConnected(NetworkConnection connection)
    {
        _connections.Add(connection);
        _scheduleManager.Run<IClientDisconnectedFromServer>(x => x.OnClientDisconnectedFromServer(connection));
    }


    public void Dispose()
    {
        StopServer();
        Disconnect();
        _scheduleManager.UnregisterAll(this);
    }
}