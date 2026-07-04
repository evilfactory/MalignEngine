using Lidgren.Network;
using MalignEngine;
using MalignEngine.Network;
using System.Net;

namespace SteamBolt;

public class SteamBolt : ISystem
{
#if SERVER
    public SteamBolt(ILoggerService loggerService,
        NetworkServer server, 
        IAssetService assetService)
    {
#elif CLIENT
    public SteamBolt(ILoggerService loggerService,
        NetworkClient client,
        IAssetService assetService)
    {
#endif

#if SERVER
        server.AddTransport(new LidgrenServerTransport(new NetPeerConfiguration("SteamBolt") { Port = 7430 }));
        server.Start();

#elif CLIENT
        client.SetTransport(new LidgrenClientTransport("SteamBolt"));
        client.Start(IPEndPoint.Parse("127.0.0.1:7430"));
#endif
    }
}