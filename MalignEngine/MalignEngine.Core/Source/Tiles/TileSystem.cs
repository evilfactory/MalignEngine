using Silk.NET.Maths;
using System.Numerics;

namespace MalignEngine;


public interface ITileSystem : IService
{
    Entity CreateTileMap(IEnumerable<TileLayer> layers);
    Entity CreateTile(Entity tileMap, TileData tileData, Vector2D<int> position);
    bool RemoveTile(Entity tileMap, string layer, Vector2D<int> position);
    bool TryGetTile(Entity tileMap, string layer, Vector2D<int> position, out Entity tile);
    IEnumerable<Entity> GetTiles(Entity tileMap, string layer);
}


public class TileSystem : ITileSystem, IUpdate
{
    private IPhysicsSystem2D _physicsSystem;
    private IEntityManager _entityManager;
    private HierarchySystem _parentSystem;
    private SceneSystem _sceneSystem;

    public TileSystem(IEntityManager entityManager, SceneSystem sceneSystem, IPhysicsSystem2D physicsSystem, HierarchySystem parentSystem)
    {
        _entityManager = entityManager;
        _sceneSystem = sceneSystem;
        _physicsSystem = physicsSystem;
        _parentSystem = parentSystem;
    }

    public Entity CreateTileMap(IEnumerable<TileLayer> layers)
    {
        Dictionary<string, TileLayer> dLayers = new Dictionary<string, TileLayer>();
        foreach (var layer in layers)
        {
            dLayers[layer.Id] = layer;
        }

        Entity entity = _entityManager.World.CreateEntity();
        entity.AddOrSet(new TileMapComponent()
        {
            Layers = dLayers
        });

        return entity;
    }

    public IEnumerable<Entity> GetTiles(Entity tileMap, string layer)
    {
        if (!tileMap.TryGet(out ComponentRef<TileMapComponent> tileMapComponent))
        {
            throw new ArgumentException("Tried to get tiles from tilemap entity but tilemap entity is not a tilemap entity");
        }

        if (!tileMapComponent.Value.Layers.TryGetValue(layer, out TileLayer? tileLayer))
        {
            throw new ArgumentException("Tile Layer not found");
        }

        return tileLayer.Tiles.Values;
    }

    public bool RemoveTile(Entity tileMap, string layer, Vector2D<int> position)
    {
        if (!tileMap.TryGet(out ComponentRef<TileMapComponent> tileMapComponent))
        {
            throw new ArgumentException("Tried to get tiles from tilemap entity but tilemap entity is not a tilemap entity");
        }

        if (!tileMapComponent.Value.Layers.TryGetValue(layer, out TileLayer tileLayer))
        {
            throw new ArgumentException("Tile Layer not found");
        }

        if (tileLayer.TryGetTile(position, out Entity entity))
        {
            _entityManager.Destroy(entity);
            tileLayer.RemoveTile(position);

            return true;
        }

        return false;
    }

    public Entity CreateTile(Entity tileMap, TileData tileData, Vector2D<int> position)
    {
        if (!tileMap.TryGet(out ComponentRef<TileMapComponent> tileMapComponent))
        {
            throw new ArgumentException("Tried to get tiles from tilemap entity but tilemap entity is not a tilemap entity");
        }

        if (!tileMapComponent.Value.Layers.TryGetValue(tileData.LayerId, out TileLayer? tileLayer))
        {
            throw new ArgumentException("Tile Layer not found");
        }

        if (tileLayer.TryGetTile(position, out Entity entity))
        {
            tileLayer.RemoveTile(position);
            _entityManager.Destroy(entity);
        }

        entity = _sceneSystem.Instantiate(tileData.Scene);
        entity.AddOrSet(new TilePosition() { X = position.X, Y = position.Y });
        entity.AddOrSet(new ParentOf() { Parent = tileMap });
        ref Transform transform = ref entity.Get<Transform>();
        transform.Scale = Vector3.One;
        transform.Position = new Vector3(position.X, position.Y, tileLayer.Order);
        tileLayer.SetTile(position, entity);

        if (tileLayer.HasCollision)
        {
            tileMapComponent.Value.ColliderNeedsUpdate = true;
        }

        return entity;
    }

    public bool TryGetTile(Entity tileMap, string layer, Vector2D<int> position, out Entity tile)
    {
        if (!tileMap.TryGet(out ComponentRef<TileMapComponent> tileMapComponent))
        {
            throw new ArgumentException("Tried to get tiles from tilemap entity but tilemap entity is not a tilemap entity");
        }

        if (!tileMapComponent.Value.Layers.TryGetValue(layer, out TileLayer? tileLayer))
        {
            throw new ArgumentException("Tile Layer not found");
        }

        return tileLayer.TryGetTile(position, out tile);
    }

    public void OnUpdate(float deltaTime)
    {
        List<Entity> tilemapsToUpdate = new List<Entity>();

        _entityManager.Query(new Query().Include<TileMapComponent>(), (Entity entity) =>
        {
            ref TileMapComponent tileMap = ref entity.Get<TileMapComponent>();

            if (tileMap.ColliderNeedsUpdate)
            {
                tileMap.ColliderNeedsUpdate = false;

                tilemapsToUpdate.Add(entity);
            }
        });

        foreach (Entity tilemap in tilemapsToUpdate)
        {
            if (!tilemap.Has<PhysicsBody2D>())
            {
                tilemap.AddOrSet(new PhysicsBody2D() { BodyType = PhysicsBodyType.Static });
            }

            ref PhysicsBody2D body = ref tilemap.Get<PhysicsBody2D>();

            List<FixtureData2D> fixtures = new List<FixtureData2D>();

            foreach ((Vector2D<int> position, Entity tile) in tilemap.Get<TileMapComponent>().Layers.SelectMany(x => x.Value.Tiles))
            {
                fixtures.Add(new FixtureData2D(new RectangleShape2D(1, 1, new Vector2(position.X, position.Y)), 0, 0.5f, 0));
            }

            body.Fixtures = fixtures.ToArray();
        }
    }
}
