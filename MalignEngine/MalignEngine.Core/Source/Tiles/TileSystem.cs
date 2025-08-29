using Silk.NET.Maths;
using System.Numerics;

namespace MalignEngine;

public class TileNeighbourChangedEvent : ComponentEventArgs { }

public interface ITileSystem : IService
{
    EntityRef CreateTileMap(IEnumerable<TileLayer> layers);
    EntityRef CreateTile(EntityRef tileMap, TileData tileData, Vector2D<int> position);
    bool RemoveTile(EntityRef tileMap, string layer, Vector2D<int> position);
    bool TryGetTile(EntityRef tileMap, string layer, Vector2D<int> position, out EntityRef tile);
    IEnumerable<EntityRef> GetTiles(EntityRef tileMap, string layer);
}

public class TileSystem : ITileSystem, IUpdate
{
    private IEventService _eventService;
    private IEntityManager _entityManager;
    private SceneSystem _sceneSystem;

    public TileSystem(IEntityManager entityManager, IEventService eventService, SceneSystem sceneSystem)
    {
        _entityManager = entityManager;
        _eventService = eventService;
        _sceneSystem = sceneSystem;
    }

    public EntityRef CreateTileMap(IEnumerable<TileLayer> layers)
    {
        Dictionary<string, TileLayer> dLayers = new Dictionary<string, TileLayer>();
        foreach (var layer in layers)
        {
            dLayers[layer.Id] = layer;
        }

        EntityRef entity = _entityManager.World.CreateEntity();
        entity.Add(new TileMapComponent()
        {
            layers = dLayers
        });

        return entity;
    }

    public IEnumerable<EntityRef> GetTiles(EntityRef tileMap, string layer)
    {
        if (!tileMap.TryGet(out TileMapComponent tileMapComponent))
        {
            throw new ArgumentException("Tried to get tiles from tilemap entity but tilemap entity is not a tilemap entity");
        }

        if (!tileMapComponent.layers.TryGetValue(layer, out TileLayer tileLayer))
        {
            throw new ArgumentException("Tile Layer not found");
        }

        return tileLayer.Tiles.Values;
    }

    public bool RemoveTile(EntityRef tileMap, string layer, Vector2D<int> position)
    {
        if (!tileMap.TryGet(out TileMapComponent tileMapComponent))
        {
            throw new ArgumentException("Tried to get tiles from tilemap entity but tilemap entity is not a tilemap entity");
        }

        if (!tileMapComponent.layers.TryGetValue(layer, out TileLayer tileLayer))
        {
            throw new ArgumentException("Tile Layer not found");
        }

        if (tileLayer.TryGetTile(position, out EntityRef entity))
        {
            entity.Destroy();
            tileLayer.RemoveTile(position);

            return true;
        }

        return false;
    }

    public EntityRef CreateTile(EntityRef tileMap, TileData tileData, Vector2D<int> position)
    {
        if (!tileMap.TryGet(out TileMapComponent tileMapComponent))
        {
            throw new ArgumentException("Tried to get tiles from tilemap entity but tilemap entity is not a tilemap entity");
        }

        if (!tileMapComponent.layers.TryGetValue(tileData.LayerId, out TileLayer tileLayer))
        {
            throw new ArgumentException("Tile Layer not found");
        }

        if (tileLayer.TryGetTile(position, out EntityRef entity))
        {
            tileLayer.RemoveTile(position);
            entity.Destroy();
        }

        entity = _sceneSystem.Instantiate(tileData.Scene);
        entity.Add(new TilePosition() { X = position.X, Y = position.Y });
        ref Transform transform = ref entity.AddOrGet<Transform>();
        transform.Scale = Vector3.One;
        transform.Position = new Vector3(position.X, position.Y, tileLayer.Order);
        tileLayer.SetTile(position, entity);

        if (tileLayer.HasCollision)
        {
            tileMapComponent.ColliderNeedsUpdate = true;
        }

        return entity;
    }

    public bool TryGetTile(EntityRef tileMap, string layer, Vector2D<int> position, out EntityRef tile)
    {
        if (!tileMap.TryGet(out TileMapComponent tileMapComponent))
        {
            throw new ArgumentException("Tried to get tiles from tilemap entity but tilemap entity is not a tilemap entity");
        }

        if (!tileMapComponent.layers.TryGetValue(layer, out TileLayer tileLayer))
        {
            throw new ArgumentException("Tile Layer not found");
        }

        return tileLayer.TryGetTile(position, out tile);
    }

    public void OnUpdate(float deltaTime)
    {
        List<EntityRef> tilemapsToUpdate = new List<EntityRef>();

        var query = _entityManager.World.CreateQuery().WithAll<TileMapComponent>();
        _entityManager.World.Query(in query, (EntityRef entity, ref TileMapComponent tileMap) =>
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

            foreach ((Vector2D<int> position, EntityRef tile) in tilemap.Get<TileMapComponent>().layers.SelectMany(x => x.Value.Tiles))
            {
                fixtures.Add(new FixtureData2D(new RectangleShape2D(1, 1, new Vector2(position.X, position.Y)), 0, 0.5f, 0));
            }

            body.Fixtures = fixtures.ToArray();
        }
    }
}