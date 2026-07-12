using MalignEngine;
using Silk.NET.Maths;
using System.Numerics;

namespace SteamBolt;

public class TileCollision : EntitySystem
{
    public TileCollision(IServiceContainer serviceContainer) : base(serviceContainer)
    {
    }

    public override void OnUpdate(float deltaTime)
    {
        List<Entity> tilemapsToUpdate = new List<Entity>();

        EntityManager.Query(new Query().Include<TileMapComponent>().Include<PhysicsBody2D>(), (Entity entity) =>
        {
            ref TileMapComponent tileMap = ref entity.Get<TileMapComponent>();

            if (tileMap.NeedsColliderUpdate)
            {
                tileMap.NeedsColliderUpdate = false;

                tilemapsToUpdate.Add(entity);
            }
        });

        foreach (Entity tilemap in tilemapsToUpdate)
        {
            ref PhysicsBody2D body = ref tilemap.Get<PhysicsBody2D>();

            List<FixtureData2D> fixtures = new List<FixtureData2D>();

            foreach ((Vector2D<int> position, Tile tile) in tilemap.Get<TileMapComponent>().Layers.SelectMany(x => x.Tiles))
            {
                fixtures.Add(new FixtureData2D(new RectangleShape2D(1, 1, new Vector2(position.X, position.Y)), 0, 0.5f, 0));
            }

            body.Fixtures = fixtures.ToArray();
        }
    }
}