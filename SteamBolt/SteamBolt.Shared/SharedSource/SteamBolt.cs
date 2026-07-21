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
        IEntityManager entityManager,
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
        
        Entity ship = entityManager.Create();

        Entity interior = tileSystem.CreateEmptyTileMap();

        interior.AddOrSet(new PhysicsBody2D() { BodyType = PhysicsBodyType.Static });
        interior.AddOrSet(new Transform() { Position = new Vector3(-1000f, 0, 0), Scale = Vector3.One });
        interior.AddOrSet(new TileCollisionComponent() { TileMap = interior });
        interior.AddOrSet(new PhysicsSpace() { Origin = new Vector2(-1000f, 0f) });
        interior.AddOrSet(new ShipInteriorComponent() { Ship = ship });

        Entity exterior = entityManager.Create();
        exterior.AddOrSet(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic });
        exterior.AddOrSet(new Transform() { Scale = Vector3.One });
        exterior.AddOrSet(new TileCollisionComponent() { TileMap = interior });
        exterior.AddOrSet(new TileRendererComponent() { TileMapEntity = interior });
        exterior.AddOrSet(new ShipExteriorComponent() { Ship = ship });

        for (int i = 0; i < 4; i++)
        {
            tileSystem.SetTile(
                interior,
                "wall",
                new Silk.NET.Maths.Vector2D<int>(i, 0),
                assetService.FromPath<TileList>("/Content/Tiles/TileList.xml").Asset.Definitions.First());
        }

        ship.AddOrSet(new ShipPhysicsComponent()
        {
            Interior = interior,
            Exterior = exterior
        });

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