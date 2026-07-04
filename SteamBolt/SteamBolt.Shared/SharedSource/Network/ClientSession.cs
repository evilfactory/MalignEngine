using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

public sealed class ClientSession : IClientSession
{
    public ClientId ClientId { get; private set; }

    public ClientSession(ClientId clientId)
    {
        ClientId = clientId;
    }
}