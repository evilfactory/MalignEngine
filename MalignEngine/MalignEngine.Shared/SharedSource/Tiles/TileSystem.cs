using Silk.NET.Maths;
using System.Numerics;

namespace MalignEngine;

public class TileSystem : EntitySystem
{
    [Dependency]
    private RenderingSystem RenderingSystem = default!;
    [Dependency]
    private SceneSystem SceneSystem = default!;

    public EntityRef CreateTileMap()
    {
        EntityRef entity = EntityManager.World.CreateEntity();
        entity.Add(new TileMapComponent());
        return entity;
    }

    public EntityRef CreateTile(EntityRef tileMap, int x, int y, TileMapComponent tileMapComponent = default)
    {
        Resolve(tileMap, ref tileMapComponent);

        EntityRef entity = EntityManager.World.CreateEntity();
        entity.Add(new TileComponent { X = x, Y = y });
        entity.Add(new Transform(new Vector2(x, y)));
        entity.Add(new ParentOf() { Parent = tileMap });
        tileMapComponent.tiles.Add(new Vector2D<int>(x, y), entity);
        tileMapComponent.ColliderNeedsUpdate = true;

        return entity;
    }

    public EntityRef AddTile(EntityRef tileMap, Scene scene, int x, int y, TileMapComponent tileMapComponent = default)
    {
        EntityRef tile = SceneSystem.LoadScene(scene);
        tile.Add(new ParentOf() { Parent = tileMap });
        tile.Add(new Transform(new Vector2(x, y)));
        tile.Add(new TileComponent { X = x, Y = y });
        tileMapComponent.tiles.Add(new Vector2D<int>(x, y), tile);
        tileMapComponent.ColliderNeedsUpdate = true;
        return tile;
    }

    public void RemoveTile(EntityRef tileMap, int x, int y, TileMapComponent tileMapComponent = default)
    {
        Resolve(tileMap, ref tileMapComponent);

        EntityRef entity = tileMapComponent.tiles[new Vector2D<int>(x, y)];
        tileMapComponent.tiles.Remove(new Vector2D<int>(x, y));
        EntityManager.World.Destroy(entity);

        tileMapComponent.ColliderNeedsUpdate = true;
    }

    public EntityRef GetTile(EntityRef tileMap, int x, int y, TileMapComponent tileMapComponent = default)
    {
        Resolve(tileMap, ref tileMapComponent);

        return tileMapComponent.tiles[new Vector2D<int>(x, y)];
    }

    public bool HasTile(EntityRef tileMap, int x, int y, TileMapComponent tileMapComponent = default)
    {
        Resolve(tileMap, ref tileMapComponent);

        return tileMapComponent.tiles.ContainsKey(new Vector2D<int>(x, y));
    }

    public IEnumerable<EntityRef> GetTiles(EntityRef tileMap, TileMapComponent tileMapComponent = default)
    {
        Resolve(tileMap, ref tileMapComponent);

        return tileMapComponent.tiles.Values;
    }

    public override void OnUpdate(float deltaTime)
    {
        List<EntityRef> tilemapsToUpdate = new List<EntityRef>();

        var query = EntityManager.World.CreateQuery().WithAll<TileMapComponent, Transform>();
        EntityManager.World.Query(in query, (EntityRef entity, ref TileMapComponent tileMap, ref Transform transform) =>
        {
            if (tileMap.ColliderNeedsUpdate)
            {
                tileMap.ColliderNeedsUpdate = false;

                tilemapsToUpdate.Add(entity);
            }
        });

        foreach (EntityRef tilemap in tilemapsToUpdate)
        {
            if (!tilemap.Has<PhysicsBody2D>())
            {
                tilemap.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Static });
            }

            PhysicsBody2D body = tilemap.Get<PhysicsBody2D>();

            List<FixtureData2D> fixtures = new List<FixtureData2D>();

            foreach ((Vector2D<int> position, EntityRef tile) in tilemap.Get<TileMapComponent>().tiles)
            {
                fixtures.Add(new FixtureData2D(new RectangleShape2D(1, 1, new Vector2(position.X, position.Y)), 0, 0.5f, 0));
            }

            body.Fixtures = fixtures.ToArray();
        }
    }
}