using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine;

public class SpriteXmlAssetLoader : IXmlLoader
{
    public string RootName => "Sprite";

    private IAssetService _assetService;

    public SpriteXmlAssetLoader(IAssetService assetService)
    {
        _assetService = assetService;
    }

    public Type GetAssetType() => typeof(Sprite);

    public IAsset Load(XElement element)
    {
        string texturePath = element.GetAttributeString("Texture");

        if (texturePath == null) { throw new Exception("No Texture attribute found"); }

        var texture = _assetService.FromPath<Texture2D>(texturePath);

        var rect = element.GetAttributeRectangle("Rectangle", new Rectangle(0, 0, (int)texture.Asset.Width, (int)texture.Asset.Height));
        var origin = element.GetAttributeVector2("Origin", new Vector2(0.5f, 0.5f));

        return new Sprite(texture, origin, rect);
    }

    public void Save(XElement element, IAsset asset)
    {
        throw new NotImplementedException();
    }
}
