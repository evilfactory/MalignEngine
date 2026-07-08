namespace MalignEngine.Network;

public interface INetworkService
{
    INetworkClient? Client { get; }
    INetworkServer? Server { get; }
}

public class NetworkService : INetworkService, IService
{
    public INetworkClient? Client { get; }
    public INetworkServer? Server { get; }

    public NetworkService(INetworkClient? client = null, INetworkServer? server = null)
    {
        Client = client;
        Server = server;
    }
}