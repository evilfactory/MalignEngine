using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine.Network;

/// <summary>
/// Called on the server when a connection is authenticated.
/// </summary>
public interface IClientSessionStarted : ISchedule
{
    void OnClientAuthenticated(IClientSession clientInfo);
}

public interface IClientSession
{
    ClientId ClientId { get; }
    NetworkConnection? Connection { get; }
    DateTime ConnectedAt { get; }
}

public interface IClientSessionSystem<TClient> where TClient : IClientSession
{
    IReadOnlyCollection<TClient> Clients { get; }

    bool TryGetClient(NetworkConnection connection, [NotNullWhen(returnValue: true)] out TClient? client);

    bool TryGetClient(ClientId clientId, [NotNullWhen(returnValue: true)] out TClient? client);
}

public abstract class ClientSessionSystemBase<ClientType> : BaseSystem, IClientSessionSystem<ClientType>, IConnectedToServer, IDisconnectedFromServer, IClientDisconnectedFromServer 
    where ClientType : class, IClientSession
{
    public IReadOnlyCollection<ClientType> Clients => clients;
    protected readonly List<ClientType> clients = new List<ClientType>();
    public ClientType? MyClient { get; private set; }

    protected readonly INetworkService NetworkService;

    public ClientSessionSystemBase(IServiceContainer serviceContainer, INetworkService networkService) : base(serviceContainer)
    {
        NetworkService = networkService;
    }

    protected abstract void SendSessionRequest();
    protected abstract void BroadcastClientListSync();

    protected void FinishClientSession(ClientType clientInfo)
    {
        MyClient = clientInfo;

        Logger.LogInfo($"Session accepted by server, my client = {MyClient}");
    }

    protected void AcceptConnection(NetworkConnection connection, ClientType clientInfo)
    {
        clients.Add(clientInfo);

        Logger.LogInfo($"Connection session accepted: {connection}, assigned client {clientInfo}");

        BroadcastClientListSync();

        ScheduleManager.Run<IClientSessionStarted>(x => x.OnClientAuthenticated(clientInfo));
    }

    protected void RejectConnection(NetworkConnection connection)
    {
        NetworkService.DisconnectConnection(connection);

        Logger.LogWarning($"Connection authentication rejected: {connection}");
    }

    public bool TryGetClient(NetworkConnection connection, [NotNullWhen(returnValue: true)] out ClientType? client)
    {
        client = Clients.FirstOrDefault(client => client.Connection == connection);
        return client != null;
    }

    public bool TryGetClient(ClientId clientId, [NotNullWhen(returnValue: true)] out ClientType? client)
    {
        client = Clients.FirstOrDefault(client => client.ClientId == clientId);
        return client != null;
    }

    #region Schedules
    public void OnConnectedToServer()
    {
        Logger.LogInfo("Sending session authentication request");

        SendSessionRequest();
    }

    public void OnDisconnectedFromServer()
    {
        Logger.LogInfo("Disconnected from server");

        MyClient = null;
    }

    public void OnClientDisconnectedFromServer(NetworkConnection connection)
    {
        if (!TryGetClient(connection, out ClientType? client))
        {
            return;
        }

        clients.RemoveAll(c => c.Connection == connection);

        BroadcastClientListSync();

        Logger.LogInfo($"Client {client} disconnected");
    }
    #endregion
}
