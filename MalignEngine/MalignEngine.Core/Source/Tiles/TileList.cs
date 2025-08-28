using System.Xml.Linq;
using System.Numerics;

namespace MalignEngine;

public class TileData
{
    public string SceneId { get; private set; }
    public AssetHandle<Sprite> Icon { get; private set; }

    public TileData(string sceneId, AssetHandle<Sprite> icon)
    {
        SceneId = sceneId;
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