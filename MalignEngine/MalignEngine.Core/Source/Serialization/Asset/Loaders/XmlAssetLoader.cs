using QuikGraph;
using System.Xml.Linq;

namespace MalignEngine;

public interface IXmlLoader : IService
{
    string RootName { get; }
    Type GetAssetType();
    IAsset Load(XElement element);
    void Save(XElement element, IAsset asset);
}

public class XmlAssetLoader : IAssetLoader
{
    private List<IXmlLoader> _xmlLoaders;

    public XmlAssetLoader(IEnumerable<IXmlLoader> xmlLoaders)
    {
        _xmlLoaders = xmlLoaders.ToList();
    }

    public Type GetAssetType(AssetPath assetPath)
    {
        var source = AssetSource.Get(assetPath);

        StreamReader reader = new StreamReader(source.GetStream());
        string text = reader.ReadToEnd();
        XElement element = XElement.Parse(text);

        source.Dispose();

        return GetLoader(element).GetAssetType();
    }

    public IAsset Load(AssetPath assetPath)
    {
        var source = AssetSource.Get(assetPath);

        StreamReader reader = new StreamReader(source.GetStream());
        string text = reader.ReadToEnd();
        XElement element = XElement.Parse(text);

        string rootName = element.Name.LocalName;

        bool isPlural = rootName.EndsWith("s");

        if (isPlural)
        {
            rootName = rootName.Remove(rootName.Length - 1);
        }

        IXmlLoader? xmlLoader = GetLoader(element);

        if (xmlLoader == null)
        {
            throw new Exception("XML asset loader not found");
        }

        if (isPlural)
        {
            XElement? targetElement = null;
            foreach (XElement subElement in element.Elements())
            {
                if (subElement.Attribute("id")?.Value == assetPath.Id)
                {
                    targetElement = subElement;
                    break;
                }
            }

            if (targetElement == null)
            {
                throw new Exception("Invalid id");
            }

            return xmlLoader.Load(targetElement);
        }
        else
        {
            return xmlLoader.Load(element);
        }
    }

    public bool IsCompatible(AssetPath assetPath)
    {
        return assetPath.Extension == "xml";
    }

    public IEnumerable<string> GetSubIds(AssetPath assetPath)
    {
        var source = AssetSource.Get(assetPath);

        StreamReader reader = new StreamReader(source.GetStream());
        string text = reader.ReadToEnd();
        XElement element = XElement.Parse(text);

        source.Dispose();

        return Enumerable.Empty<string>();
    }

    private IXmlLoader? GetLoader(XElement element)
    {
        return _xmlLoaders.Where(x => x.RootName == element.Name).FirstOrDefault();
    }

    public void Save(AssetPath assetPath, IAsset asset)
    {
        throw new NotImplementedException();
    }
}

