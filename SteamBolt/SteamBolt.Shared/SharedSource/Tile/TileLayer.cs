using MalignEngine;
using Silk.NET.Maths;

namespace SteamBolt;

public class TileChunk
{
    public const int ChunkSize = 32;

    public Point Position;

    public Tile[] Tiles;

    public TileChunk()
    {
        Tiles = new Tile[ChunkSize];
    }
}

public class TileLayer
{
    public string LayerId { get; set; } = "";
    public byte Order;
    public bool HasCollision;
    public Dictionary<Point, TileChunk> Chunks = [];

    public TileLayer(string layerId, byte order, bool hasCollision)
    {
        LayerId = layerId;
        Order = order;
        HasCollision = hasCollision;
    }


}