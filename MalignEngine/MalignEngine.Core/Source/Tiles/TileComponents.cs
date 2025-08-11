using Silk.NET.Maths;

namespace MalignEngine;

public class TileMapComponent : IComponent
{
    //[Access(typeof(TileSystem), Other = AccessPermissions.None)]
    //public Dictionary<string, TileLayer> layers;

    public bool ColliderNeedsUpdate = true;
}

public struct TileComponent : IComponent
{
    public int X;
    public int Y;
}

[Serializable]
public struct TileLayerComponent : IComponent
{
    [DataField("Layer")]
    public string Layer;
}