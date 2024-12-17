using Silk.NET.Maths;

namespace MalignEngine;

public class TileMapComponent : IComponent
{
    [Access(typeof(TileSystem), Other = AccessPermissions.None)]
    public Dictionary<Vector2D<int>, EntityRef> tiles = new Dictionary<Vector2D<int>, EntityRef>();

    public bool ColliderNeedsUpdate = true;
}

public struct TileComponent : IComponent
{
    public int X;
    public int Y;
}