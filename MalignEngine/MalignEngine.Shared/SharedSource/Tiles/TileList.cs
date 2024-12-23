using System.Xml.Linq;

namespace MalignEngine;

public class TileData
{
    public string SceneId { get; private set; }
    public Texture2D Icon { get; private set; }

    public TileData(string sceneId, Texture2D icon)
    {
        SceneId = sceneId;
        Icon = icon;

    }
}

public class TileList : IAsset
{
    public TileData[] Tiles { get; private set; }
    public string AssetPath { get; set; }

    private XElement data;

    public TileList(XElement data)
    {
        this.data = data;

        Tiles = data.Elements().ToArray().Select(x => new TileData(x.Attribute("Scene")?.Value, Texture2D.Load(x.Attribute("Icon")?.Value))).ToArray();
    }

    public static TileList Load(string assetPath)
    {
        string fileText = File.ReadAllText(assetPath);

        TileList tileList = new TileList(XElement.Parse(fileText));
        tileList.AssetPath = assetPath;
        return tileList;
    }
}