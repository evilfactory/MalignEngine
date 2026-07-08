using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

public class SessionHandler : ISessionHandler
{
    private class AuthData : INetworkSerializable
    {
        public StringClientId ClientId { get; set; } = default!;

        public void Deserialize(IReadMessage message)
        {
            ClientId = new StringClientId(message.ReadString());
        }

        public void Serialize(IWriteMessage message)
        {
            message.WriteString(ClientId.StringRepresentation);
        }
    }

    public byte[] CreateAuthData()
    {
        var writeMessage = new WriteOnlyMessage();
        var authDataMessage = new AuthData() { ClientId = new StringClientId(RandomNameGenerator.Generate()) };
        authDataMessage.Serialize(writeMessage);
        return writeMessage.Buffer;
    }

    public byte[] CreateAuthSuccessData(IClientSession session)
    {
        var writeMessage = new WriteOnlyMessage();
        var authDataMessage = new AuthData() { ClientId = (StringClientId)session.ClientId };
        authDataMessage.Serialize(writeMessage);
        return writeMessage.Buffer;
    }

    public IClientSession? HandleAuth(NetworkConnection connection, byte[] authData)
    {
        AuthData authDataMessage = new AuthData();
        authDataMessage.Deserialize(new ReadOnlyMessage(authData, false, 0, authData.Length));

        return new ClientSession(authDataMessage.ClientId);
    }

    public ClientId HandleAuthSuccess(byte[] authData)
    {
        AuthData authDataMessage = new AuthData();
        authDataMessage.Deserialize(new ReadOnlyMessage(authData, false, 0, authData.Length));

        return authDataMessage.ClientId;
    }
}