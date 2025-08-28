using System.Xml.Linq;
using System.Numerics;

namespace MalignEngine;

public class TileData
{
    public Scene Scene { get; private set; }
    public string LayerId { get; private set; }
    public AssetHandle<Sprite> Icon { get; private set; }

    public TileData(Scene scene, string layerId, AssetHandle<Sprite> icon)
    {
        Scene = scene;
        LayerId = layerId;
        Icon = icon;
    }
}

public class TileList : IAsset
{
    public IReadOnlyList<TileData> Tiles { get; private set; }

    public TileList(IEnumerable<TileData> data)
    {
        Tiles = data.ToList();
    }
}