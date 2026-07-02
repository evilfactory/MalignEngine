using QuikGraph;
using System.Xml.Linq;

namespace MalignEngine;

public abstract class XmlAssetLoader<T> : IAssetLoader where T : IAsset
{
    public IReadOnlyCollection<Type> AssetTypes => [typeof(T)];

    public IAsset Load(Stream stream)
    {
        var document = XDocument.Load(stream);

        return Load(document);
    }

    public abstract T Load(XDocument document);

    public void Save(Stream stream, IAsset asset)
    {
        throw new NotImplementedException();
    }
}