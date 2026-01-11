using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine.Network;

/// <summary>
/// Called on the server when a connection is authenticated.
/// </summary>
public interface IClientAuthenticatedToServer : ISchedule
{
    void OnClientAuthenticated(IClientInfo clientInfo);
}

/// <summary>
/// Called on the server when a connection is authenticated.
/// </summary>
public interface IAuthenticatedToServer : ISchedule
{
    void OnAuthenticated(IClientInfo clientInfo);
}

public interface IClientInfo
{
    NetworkConnection Connection { get; }
    ClientId ClientId { get; }
}

public abstract class ConnectionAuthenticationSystem<ClientType, AuthRequestNetMessage, AuthResponseNetMessage> 
    : BaseSystem, IConnectedToServer, IDisconnectedFromServer, IClientDisconnectedFromServer
    where ClientType : class, IClientInfo
    where AuthRequestNetMessage : NetMessage 
    where AuthResponseNetMessage : NetMessage
{
    public IEnumerable<ClientType> Clients => clients;
    public ClientType? MyClient { get; private set; }

    private readonly List<ClientType> clients;

    private readonly INetworkService _networkService;

    public ConnectionAuthenticationSystem(IServiceContainer serviceContainer, INetworkService networkService) : base(serviceContainer)
    {
        clients = new List<ClientType>();

        _networkService = networkService;
        _networkService.RegisterNetMessage<AuthRequestNetMessage>(ServerReceiveAuthRequest);
        _networkService.RegisterNetMessage<AuthResponseNetMessage>(ClientReceiveAuthResponse);
    }

    public abstract void ClientSendAuthMessage();
    public abstract void ServerReceiveAuthRequest(AuthRequestNetMessage netMessage);
    public abstract void ClientReceiveAuthResponse(AuthResponseNetMessage netMessage);

    protected void ClientFinishAuthentication(ClientType clientInfo)
    {
        MyClient = clientInfo;

        ScheduleManager.Run<IAuthenticatedToServer>(x => x.OnAuthenticated(clientInfo));
    }

    protected void AcceptConnection(NetworkConnection connection, ClientType clientInfo)
    {
        clients.Add(clientInfo);

        Logger.LogInfo($"Connection authentication accepted: {connection}, assigned client id {clientInfo.ClientId}");

        ScheduleManager.Run<IClientAuthenticatedToServer>(x => x.OnClientAuthenticated(clientInfo));
    }

    protected void RejectConnection(NetworkConnection connection)
    {
        _networkService.DisconnectConnection(connection);

        Logger.LogWarning($"Connection authentication rejected: {connection}");
    }

    public void OnConnectedToServer()
    {
        Logger.LogInfo("Sending connection authentication request");

        ClientSendAuthMessage();
    }

    public void OnDisconnectedFromServer()
    {
        Logger.LogInfo("Disconnected from server");

        MyClient = null;
    }

    public void OnClientDisconnectedFromServer(NetworkConnection connection)
    {
        ClientType? client = clients.FirstOrDefault(x => x.Connection == connection);

        if (client == null) { return; }

        clients.Remove(client);

        Logger.LogInfo($"Client {client} disconnected");
    }
}
