using Silk.NET.Maths;
using System.Numerics;

namespace MalignEngine;

public class TileLayer
{
    public string LayerName;
    public int Order;
    public bool HasCollision;

    internal Dictionary<Vector2D<int>, EntityRef> tiles = new Dictionary<Vector2D<int>, EntityRef>();

    public TileLayer(string layerName, int order, bool hasCollision)
    {
        LayerName = layerName;
        Order = order;
        HasCollision = hasCollision;
    }
}

public class TileNeighbourChangedEvent : ComponentEventArgs { }

public class TileSystem : EntitySystem
{
    [Dependency]
    private IRenderer2D IRenderingService = default!;
    [Dependency]
    private SceneSystem SceneSystem = default!;

    public EntityRef CreateTileMap(WorldRef world, TileLayer[] layers)
    {
        EntityRef entity = world.CreateEntity();

        Dictionary<string, TileLayer> layerData = new Dictionary<string, TileLayer>();
        foreach (TileLayer layer in layers) 
        {
            layerData.Add(layer.LayerName, layer);
        }

        entity.Add(new TileMapComponent() { layers = layerData });
        return entity;
    }

    public EntityRef SetTile(EntityRef tileMap, Scene scene, int x, int y, TileMapComponent tileMapComponent = default)
    {
        Resolve(tileMap, ref tileMapComponent);

        EntityRef tile = SceneSystem.Instantiate(scene);
        tile.Add(new ParentOf() { Parent = tileMap });
        tile.Add(new Transform(new Vector2(x, y)));
        tile.Add(new TileComponent { X = x, Y = y });

        if (!tile.Has<TileLayerComponent>())
        {
            tile.Add(new TileLayerComponent() { Layer = tileMapComponent.layers.First().Key });
        }

        string layerName = tile.Get<TileLayerComponent>().Layer;
        TileLayer layer = tileMapComponent.layers[layerName];

        if (tile.Has<SpriteRenderer>())
        {
            tile.Get<SpriteRenderer>().Layer = layer.Order;
        }

        if (HasTile(tileMap, x, y, layerName, tileMapComponent))
        {
            RemoveTile(tileMap, x, y, layerName, tileMapComponent);
        }

        layer.tiles.Add(new Vector2D<int>(x, y), tile);
     
        tileMapComponent.ColliderNeedsUpdate = true;

        foreach (EntityRef neighbour in GetNeighbourTiles(tileMap, x, y, layerName, tileMapComponent))
        {
            EventService.Get<ComponentEventChannel<TileNeighbourChangedEvent>>().Raise(neighbour, new TileNeighbourChangedEvent());
        }

        return tile;
    }

    public void RemoveTile(EntityRef tileMap, int x, int y, string layerName, TileMapComponent tileMapComponent = default)
    {
        Resolve(tileMap, ref tileMapComponent);

        TileLayer layer = tileMapComponent.layers[layerName];

        EntityRef entity = layer.tiles[new Vector2D<int>(x, y)];
        layer.tiles.Remove(new Vector2D<int>(x, y));
        EntityManager.World.Destroy(entity);

        tileMapComponent.ColliderNeedsUpdate = true;
    }

    public EntityRef[] GetNeighbourTiles(EntityRef tilemMap, int x, int y, string layerName, TileMapComponent tileMapComponent = default)
    {
        Resolve(tilemMap, ref tileMapComponent);

        List<EntityRef> neighbours = new List<EntityRef>();

        if (HasTile(tilemMap, x - 1, y, layerName, tileMapComponent))
        {
            neighbours.Add(GetTile(tilemMap, x - 1, y, layerName, tileMapComponent));
        }

        if (HasTile(tilemMap, x + 1, y, layerName, tileMapComponent))
        {
            neighbours.Add(GetTile(tilemMap, x + 1, y, layerName, tileMapComponent));
        }

        if (HasTile(tilemMap, x, y - 1, layerName, tileMapComponent))
        {
            neighbours.Add(GetTile(tilemMap, x, y - 1, layerName, tileMapComponent));
        }

        if (HasTile(tilemMap, x, y + 1, layerName, tileMapComponent))
        {
            neighbours.Add(GetTile(tilemMap, x, y + 1, layerName, tileMapComponent));
        }

        return neighbours.ToArray();
    }

    public EntityRef GetTile(EntityRef tileMap, int x, int y, string layerName, TileMapComponent tileMapComponent = default)
    {
        Resolve(tileMap, ref tileMapComponent);

        TileLayer layer = tileMapComponent.layers[layerName];

        return layer.tiles[new Vector2D<int>(x, y)];
    }

    public bool HasTile(EntityRef tileMap, int x, int y, string layerName, TileMapComponent tileMapComponent = default)
    {
        Resolve(tileMap, ref tileMapComponent);

        TileLayer layer = tileMapComponent.layers[layerName];

        return layer.tiles.ContainsKey(new Vector2D<int>(x, y));
    }

    public IEnumerable<EntityRef> GetTiles(EntityRef tileMap, string layerName, TileMapComponent tileMapComponent = default)
    {
        Resolve(tileMap, ref tileMapComponent);

        TileLayer layer = tileMapComponent.layers[layerName];

        return layer.tiles.Values;
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

            foreach ((Vector2D<int> position, EntityRef tile) in tilemap.Get<TileMapComponent>().layers.SelectMany(x => x.Value.tiles))
            {
                fixtures.Add(new FixtureData2D(new RectangleShape2D(1, 1, new Vector2(position.X, position.Y)), 0, 0.5f, 0));
            }

            body.Fixtures = fixtures.ToArray();
        }
    }
}