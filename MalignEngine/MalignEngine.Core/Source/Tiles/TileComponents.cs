using Silk.NET.Maths;

namespace MalignEngine;

public class TileMapComponent : IComponent
{
    [Access(typeof(TileSystem), Other = AccessPermissions.None)]
    public Dictionary<string, TileLayer> layers;

    public bool ColliderNeedsUpdate;
}

[Serializable]
public struct TilePosition : IComponent
{
    [DataField("X", save: true)]
    public int X;
    [DataField("Y", save: true)]
    public int Y;
}