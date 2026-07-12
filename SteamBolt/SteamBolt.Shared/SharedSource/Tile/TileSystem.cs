using MalignEngine;
using Silk.NET.Maths;

namespace SteamBolt;


public class TileMapComponent : IComponent
{
    public bool NeedsColliderUpdate;
    public TileLayer[] Layers;

    public TileMapComponent(IEnumerable<TileLayer> layers)
    {
        Layers = layers.ToArray();
    }
}

public interface ITileSystem
{
    Entity CreateEmptyTileMap();
    void SetTile(TileMapComponent tileMap, string layerId, Vector2D<int> position, TileDefinition tileDefinition);
    void RemoveTile(TileMapComponent tileMap, string layerId, Vector2D<int> position);
    Tile? GetTile(TileMapComponent tileMap, string layerId, Vector2D<int> position);
}

public class TileSystem : EntitySystem, ITileSystem
{
    public TileSystem(IServiceContainer serviceContainer) : base(serviceContainer)
    {
    }

    public Entity CreateEmptyTileMap()
    {
        Entity entity = EntityManager.Create();
        entity.AddOrSet(new TileMapComponent([
            new TileLayer("background", 0, false),
            new TileLayer("wall", 1, true),
            new TileLayer("foreground", 2, false)
            ]));
        return entity;
    }

    public void SetTile(TileMapComponent tileMap, string layerId, Vector2D<int> position, TileDefinition tileDefinition)
    {
        TileLayer layer = tileMap.Layers.First(layer => layer.LayerId == layerId);

        if (layer.HasCollision) { tileMap.NeedsColliderUpdate = true; }

        layer.Tiles[position] = new Tile()
        {
            Definition = tileDefinition
        };
    }

    public void RemoveTile(TileMapComponent tileMap, string layerId, Vector2D<int> position)
    {
        TileLayer layer = tileMap.Layers.First(layer => layer.LayerId == layerId);

        if (layer.HasCollision) { tileMap.NeedsColliderUpdate = true; }

        layer.Tiles.Remove(position);
    }

    public Tile? GetTile(TileMapComponent tileMap, string layerId, Vector2D<int> position)
    {
        TileLayer layer = tileMap.Layers.First(layer => layer.LayerId == layerId);

        if (layer.Tiles.TryGetValue(position, out Tile tile))
        {
            return tile;
        }

        return null;
    }
}