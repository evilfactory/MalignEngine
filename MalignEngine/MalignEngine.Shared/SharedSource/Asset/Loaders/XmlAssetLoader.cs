using System.Xml.Linq;

namespace MalignEngine;

public class XmlAssetLoaderFactory : IAssetFileLoaderFactory
{
    public bool CanLoadExtension(string extension)
    {
        return extension == ".xml";
    }

    public IAssetFileLoader[] CreateLoaders(string file)
    {
        string fileText = File.ReadAllText(file);
        XElement element = XElement.Parse(fileText);

        string rootName = element.Name.LocalName;

        bool isPlural = rootName.EndsWith("s");

        if (isPlural)
        {
            rootName = rootName.Remove(rootName.Length - 1);
        }

        // Search all types in all assemblies for a type with the same name as the root element
        Type? type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .FirstOrDefault(p => p.Name == rootName);

        if (type == null)
        {
            throw new Exception($"No type found with name {rootName}");
        }

        if (!type.IsSubclassOf(typeof(XmlAsset)))
        {
            throw new Exception($"Type {type.Name} does not inherit from XmlAsset");
        }

        if (isPlural)
        {
            // Each sub element is a separate asset
            List<IAssetFileLoader> loaders = new List<IAssetFileLoader>();

            foreach (XElement subElement in element.Elements())
            {
                loaders.Add(new XmlAssetLoader(new AssetPath(file), type, subElement));
            }

            return loaders.ToArray();
        }
        else
        {
            return new IAssetFileLoader[] { new XmlAssetLoader(new AssetPath(file), type, element) };
        }
    }
}

public class XmlAssetLoader : AssetFileLoader
{
    private Type type;
    private XElement elementToLoad;

    public XmlAssetLoader(AssetPath assetPath, Type type, XElement element) : base(assetPath)
    {
        elementToLoad = element;
        this.type = type;
    }

    public override IAsset Load()
    {
        XmlAsset asset = (XmlAsset)Activator.CreateInstance(type);

        asset.Load(elementToLoad);

        return (IAsset)asset;
    }
}