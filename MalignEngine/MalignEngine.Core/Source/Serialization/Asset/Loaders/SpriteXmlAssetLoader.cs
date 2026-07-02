using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine;

public class SpriteXmlAssetLoader : XmlAssetLoader<Sprite>
{
    private IAssetService _assetService;

    public SpriteXmlAssetLoader(IAssetService assetService)
    {
        _assetService = assetService;
    }

    public override Sprite Load(XDocument document)
    {
        XElement element = document.Root!;

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
