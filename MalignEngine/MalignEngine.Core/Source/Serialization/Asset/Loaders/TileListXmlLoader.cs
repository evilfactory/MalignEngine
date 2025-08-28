using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine;

public class TileListXmlLoader : IXmlLoader
{
    public string RootName => "TileList";

    private IAssetService _assetService;

    public TileListXmlLoader(IAssetService assetService)
    {
        _assetService = assetService;
    }

    public Type GetAssetType() => typeof(Sprite);

    public IAsset Load(XElement element)
    {
        List<TileData> tiles = new List<TileData>();

        foreach (XElement child in element.Elements())
        {
            string? sceneId = child.Attribute("Scene")?.Value;
            string? iconPath = child.Attribute("Icon")?.Value;

            if (sceneId == null || iconPath == null) { continue; }

            AssetHandle<Sprite> sprite = _assetService.FromPath<Sprite>(iconPath);

            tiles.Add(new TileData(sceneId, sprite));
        }

        TileList tileList = new TileList(tiles);

        return tileList;
    }

    public void Save(XElement element, IAsset asset)
    {
        throw new NotImplementedException();
    }
}
