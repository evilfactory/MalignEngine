using MalignEngine.Network;

namespace SteamBolt;

public class Client : IClientSession
{
    public NetworkConnection? Connection { get; private set; }
    public ClientId ClientId { get; private set; }
    public DateTime ConnectedAt { get; init; }

    public Client(NetworkConnection? connection, ClientId clientId)
    {
        Connection = connection;
        ClientId = clientId;
        ConnectedAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"Client({ClientId.StringRepresentation})";
    }
}