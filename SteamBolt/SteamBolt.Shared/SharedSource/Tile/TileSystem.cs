using MalignEngine;
using Silk.NET.Maths;

namespace SteamBolt;

public struct UpdateTileEvent : IEvent
{
    public Entity TileMapEntity;
    public TileMapComponent TileMapComponent;
    public TileLayer TileLayer;
    public Vector2D<int> TilePosition;
    public Tile? NewTile;
}

public class TileMapComponent : IComponent
{
    public TileLayer[] Layers;

    public TileMapComponent(IEnumerable<TileLayer> layers)
    {
        Layers = layers.ToArray();
    }
}

public interface ITileSystem
{
    Entity CreateEmptyTileMap();
    void SetTile(Entity tileMapEntity, string layerId, Vector2D<int> position, TileDefinition tileDefinition);
    void RemoveTile(Entity tileMapEntity, string layerId, Vector2D<int> position);
    Tile? GetTile(Entity tileMapEntity, string layerId, Vector2D<int> position);
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

    public void SetTile(Entity tileMapEntity, string layerId, Vector2D<int> position, TileDefinition tileDefinition)
    {
        TileMapComponent tileMapComponent = tileMapEntity.Get<TileMapComponent>();
        TileLayer layer = tileMapComponent.Layers.First(layer => layer.LayerId == layerId);

        layer.Tiles[position] = new Tile()
        {
            Definition = tileDefinition
        };

        EventService.Send(new UpdateTileEvent()
        {
            TileMapEntity = tileMapEntity,
            TileMapComponent = tileMapComponent,
            TileLayer = layer,
            TilePosition = position,
            NewTile = layer.Tiles[position]
        });
    }

    public void RemoveTile(Entity tileMapEntity, string layerId, Vector2D<int> position)
    {
        TileMapComponent tileMapComponent = tileMapEntity.Get<TileMapComponent>();
        TileLayer layer = tileMapComponent.Layers.First(layer => layer.LayerId == layerId);

        layer.Tiles.Remove(position);

        EventService.Send(new UpdateTileEvent()
        {
            TileMapEntity = tileMapEntity,
            TileMapComponent = tileMapComponent,
            TileLayer = layer,
            TilePosition = position,
            NewTile = null
        });
    }

    public Tile? GetTile(Entity tileMapEntity, string layerId, Vector2D<int> position)
    {
        TileMapComponent tileMapComponent = tileMapEntity.Get<TileMapComponent>();
        TileLayer layer = tileMapComponent.Layers.First(layer => layer.LayerId == layerId);

        if (layer.Tiles.TryGetValue(position, out Tile tile))
        {
            return tile;
        }

        return null;
    }
}