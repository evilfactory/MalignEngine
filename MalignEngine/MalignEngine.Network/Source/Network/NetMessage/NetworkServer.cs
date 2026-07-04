namespace MalignEngine.Network;

public interface INetworkServer
{
    bool IsRunning { get; }
    IEnumerable<NetworkConnection> Connections { get; }

    void Register<T>(Action<NetworkConnection, T> handler) where T : NetMessage;
    void Unregister<T>() where T : NetMessage;
    void Start();
    void Stop();
    void Send<T>(NetworkConnection connection, T message) where T : NetMessage;
    void Broadcast<T>(T message) where T : NetMessage;
    void Disconnect(NetworkConnection connection);
    void AddTransport(IServerTransport transport);
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

public class NetworkServer : INetworkServer, IUpdate
{
    private record MessageData(Type Type, Action<NetworkConnection, NetMessage> Callback);

    public bool IsRunning { get; private set; }

    private List<IServerTransport> _transports = [];
    public IEnumerable<NetworkConnection> Connections => _connections;
    private List<NetworkConnection> _connections = [];

    private Dictionary<string, MessageData> _handles = [];

    private ILogger _logger;
    private IScheduleManager _scheduleManager;

    public NetworkServer(ILoggerService logger, IScheduleManager scheduleManager)
    {
        _logger = logger.GetSawmill("network.server");
        _scheduleManager = scheduleManager;

        scheduleManager.RegisterAll(this);
    }

    private void Transport_ClientDisconnected(NetworkConnection connection)
    {
        _connections.Remove(connection);

        _scheduleManager.Run<IClientConnectedToServer>(d => d.OnClientConnectedToServer(connection));

        _logger.LogVerbose($"Disconnected: {connection}");
    }

    private void Transport_ClientConnected(NetworkConnection connection)
    {
        _connections.Add(connection);

        _scheduleManager.Run<IClientDisconnectedFromServer>(d => d.OnClientDisconnectedFromServer(connection));

        _logger.LogVerbose($"Connected: {connection}");
    }

    private void Transport_Received(NetworkConnection connection, IReadMessage message)
    {
        string netMessageName = message.ReadString();

        if (!_handles.TryGetValue(netMessageName, out MessageData? data))
        {
            _logger.LogWarning($"Failed to find handle for net message {netMessageName}");
            return;
        }

        NetMessage netMessage = (NetMessage)Activator.CreateInstance(data.Type)!;
        netMessage.Deserialize(message);
        data.Callback(connection, netMessage);
    }

    public void AddTransport(IServerTransport transport)
    {
        _transports.Add(transport);

        transport.ClientConnected += Transport_ClientConnected;
        transport.ClientDisconnected += Transport_ClientDisconnected;
        transport.Received += Transport_Received;
    }

    public void Register<T>(Action<NetworkConnection, T> handler) where T : NetMessage
    {
        NetMessage netMessage = Activator.CreateInstance<T>();
        _handles.Add(netMessage.MsgName, 
            new MessageData(
                typeof(T), 
                (NetworkConnection connection, NetMessage message) => handler(connection, (T)message)));
    }

    public void Unregister<T>() where T : NetMessage
    {
        NetMessage netMessage = Activator.CreateInstance<T>();
        _handles.Remove(netMessage.MsgName);
    }

    public void Send<T>(NetworkConnection connection, T message) where T : NetMessage
    {
        ITransport? transport = _transports.FirstOrDefault(t => t == connection.Transport);
        
        if (transport is not IServerTransport serverTransport)
        {
            throw new InvalidOperationException("Transport for connection not found");
        }

        WriteOnlyMessage payload = new WriteOnlyMessage();
        payload.WriteString(message.MsgName);
        message.Serialize(payload);
        serverTransport.Send(connection, payload, message.Channel);
    }

    public void Broadcast<T>(T message) where T : NetMessage
    {
        foreach (var connection in _connections)
        {
            Send(connection, message);
        }
    }

    public void Disconnect(NetworkConnection connection)
    {
        ITransport? transport = _transports.FirstOrDefault(t => t == connection.Transport);

        if (transport is not IServerTransport serverTransport)
        {
            throw new InvalidOperationException("Transport for connection not found");
        }

        serverTransport.Disconnect(connection);
    }

    public void Start()
    {
        foreach (var transport in _transports)
        {
            transport.Start();
        }
        IsRunning = true;

        _scheduleManager.Run<IServerStart>(d => d.OnServerStarted());
    }

    public void Stop()
    {
        IsRunning = false;
        foreach (var transport in _transports)
        {
            transport.Stop();
        }

        _scheduleManager.Run<IServerStop>(d => d.OnServerStopped());
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var transport in _transports)
        {
            transport.Update();
        }
    }
}