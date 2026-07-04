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

        assetService.Mount("/Content/", new FileAssetSource("Content/"));

        AssetManifest manifest = new AssetManifest([
            (typeof(Scene), "/Content/Player.xml")
        ]);

        assetService.PreLoadAsync(manifest).Wait();

#if SERVER
        server.AddTransport(new LidgrenServerTransport(new NetPeerConfiguration("SteamBolt") { Port = 7430 }));
        server.Start();

#elif CLIENT
        client.SetTransport(new LidgrenClientTransport("SteamBolt"));
        client.Start(IPEndPoint.Parse("127.0.0.1:7430"));
#endif
    }
}