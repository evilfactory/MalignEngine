using System.Xml.Linq;

namespace MalignEngine;

public class XmlAssetLoader : IAssetLoader
{
    public XmlAssetLoader()
    {
    }

    public Type GetAssetType(AssetPath assetPath)
    {
        var source = AssetSource.Get(assetPath);

        StreamReader reader = new StreamReader(source.GetStream());
        string text = reader.ReadToEnd();
        XElement element = XElement.Parse(reader.ReadToEnd());
        return GetTypeFrom(element);
    }

    public IAsset Load(AssetPath assetPath)
    {
        var source = AssetSource.Get(assetPath);

        StreamReader reader = new StreamReader(source.GetStream());
        string text = reader.ReadToEnd();
        XElement element = XElement.Parse(reader.ReadToEnd());

        string rootName = element.Name.LocalName;

        bool isPlural = rootName.EndsWith("s");

        if (isPlural)
        {
            rootName = rootName.Remove(rootName.Length - 1);
        }

        Type type = GetTypeFrom(element);

        XmlAsset? xmlAsset = null;
        XElement? foundElement = null;

        if (isPlural)
        {
            // Each sub element is a separate asset
            foreach (XElement subElement in element.Elements())
            {
                if (subElement.Attribute("id")?.Value == assetPath.Id)
                {
                    xmlAsset = (XmlAsset)Activator.CreateInstance(type);
                    foundElement = subElement;
                    break;
                }
            }
        }
        else
        {
            xmlAsset = (XmlAsset)Activator.CreateInstance(type);
            foundElement = element;
        }

        if (xmlAsset == null)
        {
            throw new Exception("No xml asset found");
        }

        xmlAsset.Load(foundElement);

        return xmlAsset;
    }

    public bool IsCompatible(AssetPath assetPath)
    {
        return assetPath.Extension == ".xml";
    }

    public IEnumerable<string> GetSubIds(AssetPath assetPath)
    {
        throw new NotImplementedException();
    }

    private Type GetTypeFrom(XElement element)
    {
        // Search all types in all assemblies for a type with the same name as the root element
        Type? type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .FirstOrDefault(p => p.Name == element.Name);

        if (type == null)
        {
            throw new Exception($"No type found with name {element.Name}");
        }

        if (!type.IsSubclassOf(typeof(XmlAsset)))
        {
            throw new Exception($"Type {type.Name} does not inherit from XmlAsset");
        }

        return type;
    }
}

