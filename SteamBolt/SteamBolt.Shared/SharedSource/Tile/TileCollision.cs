using MalignEngine;
using Silk.NET.Maths;
using System.Numerics;

namespace SteamBolt;

public struct TileCollisionComponent : IComponent
{
    public Entity TileMap;
    public bool NeedCollisionRebuild;
}

public class TileCollision : EntitySystem
{
    private EventReader<UpdateTileEvent> UpdateTileEventReader;

    public TileCollision(IServiceContainer serviceContainer) : base(serviceContainer)
    {
        UpdateTileEventReader = EventService.GetReader<UpdateTileEvent>();
    }

    public override void OnUpdate(float deltaTime)
    {
        List<(Entity, TileMapComponent)> tilemapsToUpdate = new List<(Entity, TileMapComponent)>();

        HashSet<TileMapComponent> tileMapsNeedingUpdate = [];

        foreach (var ev in UpdateTileEventReader.Read())
        {
            if (ev.TileLayer.HasCollision)
            {
                tileMapsNeedingUpdate.Add(ev.TileMapComponent);
            }
        }

        EntityManager.Query(new Query().Include<TileCollisionComponent>().Include<PhysicsBody2D>(), (Entity entity) =>
        {
            ref TileCollisionComponent tileCollision = ref entity.Get<TileCollisionComponent>();
            ref TileMapComponent tileMap = ref tileCollision.TileMap.Get<TileMapComponent>();

            if (tileMapsNeedingUpdate.Contains(tileMap))
            {
                tilemapsToUpdate.Add((entity, tileMap));
            }
        });

        foreach ((Entity entity, TileMapComponent tileMapComponent) in tilemapsToUpdate)
        {
            ref PhysicsBody2D body = ref entity.Get<PhysicsBody2D>();

            List<FixtureData2D> fixtures = new List<FixtureData2D>();

            foreach ((Vector2D<int> position, Tile tile) in tileMapComponent.Layers.Where(x => x.HasCollision).SelectMany(x => x.Tiles))
            {
                fixtures.Add(new FixtureData2D(new RectangleShape2D(1, 1, new Vector2(position.X, position.Y)), 0, 0.5f, 0));
            }

            body.Fixtures = fixtures.ToArray();
        }
    }
}