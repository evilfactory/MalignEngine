namespace MalignEngine.Network;

public class AuthRequestNetMessage : NetMessage
{
    public required byte[] AuthData { get; set; }

    public override void Deserialize(IReadMessage message)
    {
        int size = message.ReadInt32();
        AuthData = message.ReadBytes(size);
    }

    public override void Serialize(IWriteMessage message)
    {
        message.WriteInt32(AuthData.Length);
        message.WriteBytes(AuthData, 0, AuthData.Length);
    }
}

public class AuthSuccessNetMessage : NetMessage
{
    public required byte[] AuthData { get; set; }

    public override void Deserialize(IReadMessage message)
    {
        int size = message.ReadInt32();
        AuthData = message.ReadBytes(size);
    }

    public override void Serialize(IWriteMessage message)
    {
        message.WriteInt32(AuthData.Length);
        message.WriteBytes(AuthData, 0, AuthData.Length);
    }
}

public class ServerSessionSystem : BaseSystem, IClientDisconnectedFromServer
{
    private Dictionary<NetworkConnection, IClientSession> _sessions = [];

    private INetworkServer _server;
    private ISessionHandler _sessionHandler;

    public ServerSessionSystem(IServiceContainer serviceContainer, ISessionHandler sessionHandler, INetworkServer server) : base(serviceContainer)
    {
        _server = server;
        _sessionHandler = sessionHandler;

        server.Register<AuthRequestNetMessage>(ReceiveAuthRequest);
    }

    public bool TryGetSession<T>(NetworkConnection connection, out T? session) where T : IClientSession
    {
        if (_sessions.TryGetValue(connection, out var sessionValue))
        {
            session = (T)sessionValue;
            return true;
        }

        session = default;
        return false;
    }

    private void ReceiveAuthRequest(NetworkConnection connection, AuthRequestNetMessage message)
    {
        IClientSession? session = _sessionHandler.HandleAuth(connection, message.AuthData);

        if (session == null)
        {
            Logger.LogInfo($"Authentication rejected: {connection}");
            _server.Disconnect(connection);
            return;
        }

        _sessions.Add(connection, session);

        Logger.LogInfo($"Authentication accepted: {session.ClientId}");

        _server.Send(connection, new AuthSuccessNetMessage() { AuthData = _sessionHandler.CreateAuthSuccessData(session) });
    }

    public void OnClientDisconnectedFromServer(NetworkConnection connection)
    {
        _sessions.Remove(connection);
    }
}