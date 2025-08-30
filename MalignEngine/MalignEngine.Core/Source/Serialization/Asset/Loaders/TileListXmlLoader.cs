using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine;

public class TileListXmlLoader : IXmlLoader
{
    public string RootName => "TileList";

    private IAssetService _assetService;
    private ILogger _logger;

    public TileListXmlLoader(IAssetService assetService, ILoggerService loggerService)
    {
        _logger = loggerService.GetSawmill("loader.tilelist");
        _assetService = assetService;
    }

    public Type GetAssetType() => typeof(TileList);

    public IAsset Load(XElement element)
    {
        List<TileData> tiles = new List<TileData>();

        foreach (XElement child in element.Elements())
        {
            string? scenePath = child.Attribute("Scene")?.Value;
            string? iconPath = child.Attribute("Icon")?.Value;
            string? layerId = child.Attribute("Layer")?.Value;

            if (scenePath == null || iconPath == null || layerId == null)
            {
                _logger.LogWarning($"missing attribute {scenePath} {iconPath} {layerId}");
                continue;
            }

            AssetHandle<Scene>? originalScene = _assetService.GetHandles<Scene>().FirstOrDefault(x => x.Asset.SceneId == scenePath);

            if (originalScene == null)
            {
                _logger.LogWarning($"scene not found {scenePath}");
                continue;
            }

            AssetHandle<Sprite> sprite = _assetService.FromPath<Sprite>(iconPath);

            tiles.Add(new TileData(originalScene, layerId, sprite));
        }

        TileList tileList = new TileList(tiles);

        return tileList;
    }

    public void Save(XElement element, IAsset asset)
    {
        throw new NotImplementedException();
    }
}
