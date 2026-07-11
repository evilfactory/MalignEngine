using Lidgren.Network;
using MalignEngine;
using MalignEngine.Network;
using System.Net;

namespace SteamBolt;

public class SteamBolt : ISystem
{
    public SteamBolt(ILoggerService loggerService,
        INetworkService _network,
        IAssetService assetService,
        SceneSystem sceneSystem)
    {
        assetService.Mount("/Content/", new FileAssetSource("Content/"));

        AssetManifest manifest = new AssetManifest([
            (typeof(Scene), "/Content/Player.xml")
        ]);

        assetService.PreLoadAsync(manifest).Wait();

        Scene scene = assetService.FromPath<Scene>("/Content/Map.xml");
        Entity entity = sceneSystem.Instantiate(scene);

        if (_network.Server != null)
        {
            _network.Server.AddTransport(new LidgrenServerTransport(new NetPeerConfiguration("SteamBolt") { Port = 7430 }));
            _network.Server.Start();
        }
        else if (_network.Client != null)
        {
            _network.Client.SetTransport(new LidgrenClientTransport("SteamBolt"));
            _network.Client.Start(IPEndPoint.Parse("127.0.0.1:7430"));
        }
    }
}