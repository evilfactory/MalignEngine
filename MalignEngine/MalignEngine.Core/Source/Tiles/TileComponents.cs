using Silk.NET.Maths;

namespace MalignEngine;

public class TileMapComponent : IComponent
{
    [Access(typeof(TileSystem), Other = AccessPermissions.None)]
    public Dictionary<string, TileLayer> layers;

    public bool ColliderNeedsUpdate;
}

public struct TilePosition : IComponent
{
    public int X;
    public int Y;
    public byte Layer;
}