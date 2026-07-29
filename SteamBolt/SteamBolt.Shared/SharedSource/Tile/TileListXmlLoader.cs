using MalignEngine;
using SteamBolt;
using System.Xml.Linq;

namespace SteamBolt;

public class TileListXmlLoader : XmlAssetLoader<TileList>
{
    public string RootName => "TileList";

    private XmlSerializer _xmlSerializer;

    public TileListXmlLoader(XmlSerializer xmlSerializer)
    {
        _xmlSerializer = xmlSerializer;
    }

    public Type GetAssetType() => typeof(TileList);

    public override TileList Load(XDocument document)
    {
        XElement element = document.Root!;

        List<TileDefinition> definitions = new List<TileDefinition>();

        foreach (XElement child in element.Elements())
        {
            TileDefinition tileDefinition = new TileDefinition();
            _xmlSerializer.DeserializeObject(tileDefinition, child);
            definitions.Add(tileDefinition);
        }

        TileList tileList = new TileList(definitions);

        return tileList;
    }

    public void Save(XElement element, IAsset asset)
    {
        throw new NotImplementedException();
    }
}