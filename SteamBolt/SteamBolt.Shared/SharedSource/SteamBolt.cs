using MalignEngine;
using MalignEngine.Network;
using System.Net;

namespace SteamBolt;

public class SteamBolt : ISystem
{
    public SteamBolt(ILoggerService loggerService, INetworkService networkService)
    {
        networkService.SetTransport(new LidgrenTransport(loggerService.GetSawmill("transport")));
#if SERVER
        networkService.StartServer(IPEndPoint.Parse("127.0.0.1:7430"));
#elif CLIENT
        networkService.TryConnect(IPEndPoint.Parse("127.0.0.1:7430"));
#endif
    }
}