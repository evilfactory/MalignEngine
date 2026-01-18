using Lidgren.Network;
using MalignEngine;
using MalignEngine.Network;
using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;

namespace SteamBolt;

public class SessionRequestNetMessage : NetMessage
{
    public string ClientName { get; private set; }

    public SessionRequestNetMessage(string clientName)
    {
        ClientName = clientName;
    }

    public override void Deserialize(IReadMessage message)
    {
        ClientName = message.ReadString();
    }

    public override void Serialize(IWriteMessage message)
    {
        message.WriteString(ClientName);
    }
}

public class ClientNetData
{
    public required ClientId ClientId;

    public void Serialize(IWriteMessage message)
    {
        message.WriteString(ClientId.StringRepresentation);
    }

    public void Deserialize(IReadMessage message)
    {
        ClientId = new StringClientId(message.ReadString());
    }
}

public class SessionResponseNetMessage : NetMessage
{
    public ClientNetData? Client { get; private set; }

    public SessionResponseNetMessage(ClientNetData? client)
    {
        Client = client;
    }

    public override void Serialize(IWriteMessage message)
    {
        if (Client == null)
        {
            message.WriteBoolean(false);
        }
        else if (Client != null)
        {
            message.WriteBoolean(true);
            Client.Serialize(message);
        }
    }

    public override void Deserialize(IReadMessage message)
    {
        if (message.ReadBoolean())
        {
            Client = Activator.CreateInstance<ClientNetData>();
            Client.Deserialize(message);
        }
    }
}

public class ClientListSyncNetMessage : NetMessage
{
    public required ClientNetData[] Clients;

    public override void Serialize(IWriteMessage message)
    {
        message.WriteByte((byte)Clients.Length);
        for (int i = 0; i < Clients.Length; i++)
        {
            Clients[i].Serialize(message);
        }
    }

    public override void Deserialize(IReadMessage message)
    {
        int amount = message.ReadByte();

        Clients = new ClientNetData[amount];
        for (int i = 0; i < amount; i++)
        {
            Clients[i] = Activator.CreateInstance<ClientNetData>();
            Clients[i].Deserialize(message);
        }
    }
}

public class ClientSessionSystem : ClientSessionSystemBase<Client>
{
    public ClientSessionSystem(IServiceContainer serviceContainer, INetworkService networkService) : base(serviceContainer, networkService)
    {
        NetworkService.RegisterServerNetMessage<SessionRequestNetMessage>(ReceiveSessionRequestNetMessage);
        NetworkService.RegisterClientNetMessage<SessionResponseNetMessage>(ReceiveSessionResponseNetMessage);
        NetworkService.RegisterClientNetMessage<ClientListSyncNetMessage>(ReceiveClientListSyncNetMessage);
    }

    private void ReceiveClientListSyncNetMessage(ClientListSyncNetMessage netMessage)
    {
        clients.Clear();
        clients.AddRange(netMessage.Clients.Select(x => new Client(null, x.ClientId)));
    }

    protected override void SendSessionRequest()
    {
        NetworkService.SendToServer(new SessionRequestNetMessage(RandomNameGenerator.Generate(2, 6)));
    }

    protected override void BroadcastClientListSync()
    {
        foreach (var client in Clients)
        {
            NetworkService.SendToClient(new ClientListSyncNetMessage
            {
                Clients = Clients.Select(x => new ClientNetData { ClientId = x.ClientId }).ToArray()
            }, client.Connection!);
        }
    }

    private void ReceiveSessionRequestNetMessage(NetworkConnection connection, SessionRequestNetMessage message)
    {
        var client = new Client(connection, new StringClientId(message.ClientName));
        AcceptConnection(connection, client);
        NetworkService.SendToClient(new SessionResponseNetMessage(new ClientNetData { ClientId = client.ClientId }), connection);
    }

    private void ReceiveSessionResponseNetMessage(SessionResponseNetMessage netMessage)
    {
        if (netMessage.Client == null)
        {
            Logger.LogWarning("Failed to initiate session with the server, disconnecting.");
            NetworkService.Disconnect();
            return;
        }

        FinishClientSession(new Client(null, netMessage.Client.ClientId));
    }
}