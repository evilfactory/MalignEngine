using System.Xml.Linq;

namespace MalignEngine;

public abstract class XmlAsset
{
    public abstract void Load(XElement element);
    public abstract void Save(XElement element);
}

public abstract class XmlAsset<T> : XmlAsset, IFileLoadableAsset<T> where T : class, new()
{
    public XElement? OriginalElement { get; private set; }

    public T LoadFromPath(AssetPath assetPath)
    {
        string fileText = File.ReadAllText(assetPath);
        XElement element = XElement.Parse(fileText);

        OriginalElement = element;

        Load(element);

        return this as T;
    }
}