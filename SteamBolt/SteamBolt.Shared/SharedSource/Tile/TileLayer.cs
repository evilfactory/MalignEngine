using Silk.NET.Maths;

namespace SteamBolt;

public class TileLayer
{
    public string LayerId { get; set; } = "";
    public byte Order;
    public bool HasCollision;
    public Dictionary<Vector2D<int>, Tile> Tiles = [];

    public TileLayer(string layerId, byte order, bool hasCollision)
    {
        LayerId = layerId;
        Order = order;
        HasCollision = hasCollision;
    }
}