using Lidgren.Network;
using MalignEngine;
using MalignEngine.Network;
using System.Net;
using System.Numerics;

namespace SteamBolt;

public class SteamBolt : ISystem
{
    public SteamBolt(ILoggerService loggerService,
        INetworkService _network,
        IAssetService assetService,
        ITileSystem tileSystem,
        SceneSystem sceneSystem)
    {
        assetService.Mount("/Content/", new FileAssetSource("Content/"));

        AssetManifest manifest = new AssetManifest([
            (typeof(Scene), "/Content/Player.xml"),
            (typeof(TileList), "/Content/Tiles/TileList.xml")
        ]);

        assetService.PreLoadAsync(manifest).Wait();

        Scene scene = assetService.FromPath<Scene>("/Content/Map.xml");
        Entity entity = sceneSystem.Instantiate(scene);

        Entity tilemap = tileSystem.CreateEmptyTileMap();

        tilemap.AddOrSet(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic });
        tilemap.AddOrSet(new Transform() { Scale = Vector3.One });

        tileSystem.SetTile(
            tilemap.Get<TileMapComponent>(), 
            "wall", 
            new Silk.NET.Maths.Vector2D<int>(0, 0), 
            assetService.FromPath<TileList>("/Content/Tiles/TileList.xml").Asset.Definitions.First());

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