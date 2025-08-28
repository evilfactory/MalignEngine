using Silk.NET.Maths;

namespace MalignEngine;

public class TileLayer
{
    public string Id { get; }
    public byte Order { get; }
    public bool HasCollision { get; }

    private readonly Dictionary<Vector2D<int>, EntityRef> _tiles = new();

    public IReadOnlyDictionary<Vector2D<int>, EntityRef> Tiles => _tiles;

    internal void SetTile(Vector2D<int> pos, EntityRef entity) => _tiles[pos] = entity;
    internal void RemoveTile(Vector2D<int> pos) => _tiles.Remove(pos);
    internal bool TryGetTile(Vector2D<int> pos, out EntityRef entity) => _tiles.TryGetValue(pos, out entity);

    public TileLayer(string id, byte order, bool hasCollision)
    {
        Id = id;
        Order = order;
        HasCollision = hasCollision;
    }
}