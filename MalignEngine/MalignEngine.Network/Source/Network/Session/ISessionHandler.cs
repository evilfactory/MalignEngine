namespace MalignEngine.Network;

public interface ISessionHandler
{
    byte[] CreateAuthData();
    byte[] CreateAuthSuccessData(IClientSession session);
    IClientSession? HandleAuth(NetworkConnection connection, byte[] authData);
    ClientId HandleAuthSuccess(byte[] authData);
}