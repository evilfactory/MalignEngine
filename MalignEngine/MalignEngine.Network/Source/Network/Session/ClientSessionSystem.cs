namespace MalignEngine.Network;

public class ClientSessionSystem : BaseSystem, IConnectedToServer, IDisconnectedFromServer
{
    public bool IsAuthenticated => ClientId != null;

    public ClientId? ClientId { get; private set; }

    private INetworkClient _client;
    private ISessionHandler _sessionHandler;

    public ClientSessionSystem(IServiceContainer serviceContainer, INetworkClient client, ISessionHandler sessionHandler) : base(serviceContainer)
    {
        _client = client;
        _sessionHandler = sessionHandler;

        client.Register<AuthSuccessNetMessage>(ReceiveAuthSuccess);
    }

    private void ReceiveAuthSuccess(AuthSuccessNetMessage message)
    {
        ClientId = _sessionHandler.HandleAuthSuccess(message.AuthData);

        Logger.LogInfo($"Authentication success: {ClientId}");
    }

    public void OnConnectedToServer()
    {
        _client.Send(new AuthRequestNetMessage() { AuthData = _sessionHandler.CreateAuthData() });
    }

    public void OnDisconnectedFromServer()
    {
        ClientId = null;
    }
}