using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine;

public class DummyTextureAssetLoader : IAssetLoader
{
    public IReadOnlyCollection<Type> AssetTypes => [typeof(Texture2D)];

    public IAsset Load(Stream stream)
    {
        throw new NotImplementedException();
    }

    public void Save(Stream stream, IAsset asset)
    {
        throw new NotImplementedException();
    }
}
