using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine;

public class DummySpriteXmlAssetLoader : XmlAssetLoader<Sprite>
{
    public DummySpriteXmlAssetLoader()
    {
    }

    public override Sprite Load(XDocument document)
    {
        return new Sprite(null, new Vector2(), new Rectangle());
    }

    public void Save(XElement element, IAsset asset)
    {
        throw new NotImplementedException();
    }
}
