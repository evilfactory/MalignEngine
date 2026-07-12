using MalignEngine;
using SteamBolt;
using System.Xml.Linq;

namespace SteamBolt;

public class TileListXmlLoader : XmlAssetLoader<TileList>
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

    public override TileList Load(XDocument document)
    {
        XElement element = document.Root!;

        List<TileDefinition> definitions = new List<TileDefinition>();

        foreach (XElement child in element.Elements())
        {
            string? identifier = child.Attribute("Identifier")?.Value;
            string? spritePath = child.Attribute("Sprite")?.Value;
            string? layerId = child.Attribute("Layer")?.Value;

            if (identifier == null || spritePath == null || layerId == null)
            {
                _logger.LogWarning($"missing attribute {identifier} {spritePath} {layerId}");
                continue;
            }

            AssetHandle<Sprite> sprite = _assetService.FromPath<Sprite>(spritePath);

            definitions.Add(new TileDefinition(identifier, layerId, sprite));
        }

        TileList tileList = new TileList(definitions);

        return tileList;
    }

    public void Save(XElement element, IAsset asset)
    {
        throw new NotImplementedException();
    }
}