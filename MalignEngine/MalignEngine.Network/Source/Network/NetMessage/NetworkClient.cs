using System.Net;

namespace MalignEngine.Network;

public interface INetworkClient
{
    bool IsRunning { get; }

    void Register<T>(Action<T> handler) where T : NetMessage;
    void Unregister<T>() where T : NetMessage;
    void Start(IPEndPoint endpoint);
    void Stop();
    void Send<T>(T message) where T : NetMessage;
    void SetTransport(IClientTransport transport);
}

public interface IConnectedToServer : ISchedule
{
    void OnConnectedToServer();
}

public interface IDisconnectedFromServer : ISchedule
{
    void OnDisconnectedFromServer();
}

public class NetworkClient : INetworkClient, IUpdate
{
    private record MessageData(Type Type, Action<NetMessage> Callback);

    public bool IsRunning { get; private set; }

    private Dictionary<string, MessageData> _handles = [];

    private IClientTransport? _transport;
    private ILogger _logger;
    private IScheduleManager _scheduleManager;

    public NetworkClient(ILoggerService logger, IScheduleManager scheduleManager)
    {
        _logger = logger.GetSawmill("network.client");
        _scheduleManager = scheduleManager;

        scheduleManager.RegisterAll(this);
    }

    private void Transport_Disconnected()
    {
        IsRunning = false;

        _scheduleManager.Run<IDisconnectedFromServer>(d => d.OnDisconnectedFromServer());

        _logger.LogVerbose("Disconnected from server");
    }

    private void Transport_Connected()
    {
        IsRunning = true;

        _scheduleManager.Run<IConnectedToServer>(d => d.OnConnectedToServer());

        _logger.LogVerbose("Connected to server");
    }

    private void Transport_Received(IReadMessage message)
    {
        string netMessageName = message.ReadString();

        if (!_handles.TryGetValue(netMessageName, out MessageData? data))
        {
            _logger.LogWarning($"Failed to find handle for net message {netMessageName}");
            return;
        }

        NetMessage netMessage = (NetMessage)Activator.CreateInstance(data.Type)!;
        netMessage.Deserialize(message);
        data.Callback(netMessage);
    }

    public void SetTransport(IClientTransport transport)
    {
        _transport = transport;

        _transport.Connected += Transport_Connected;
        _transport.Disconnected += Transport_Disconnected;
        _transport.Received += Transport_Received;
    }

    public void Register<T>(Action<T> handler) where T : NetMessage
    {
        NetMessage netMessage = Activator.CreateInstance<T>();
        _handles.Add(netMessage.MsgName,
            new MessageData(
                typeof(T),
                (NetMessage message) => handler((T)message)));
    }

    public void Unregister<T>() where T : NetMessage
    {
        NetMessage netMessage = Activator.CreateInstance<T>();
        _handles.Remove(netMessage.MsgName);
    }

    public void Send<T>(T message) where T : NetMessage
    {
        WriteOnlyMessage payload = new WriteOnlyMessage();
        payload.WriteString(message.MsgName);
        message.Serialize(payload);
        _transport.Send(payload, message.Channel);
    }

    public void Disconnect()
    {
        _transport.Disconnect();
    }

    public void Start(IPEndPoint endpoint)
    {
        _transport.Connect(endpoint);
    }

    public void Stop()
    {
        _transport.Disconnect();
    }

    public void OnUpdate(float deltaTime)
    {
        _transport?.Update();
    }
}