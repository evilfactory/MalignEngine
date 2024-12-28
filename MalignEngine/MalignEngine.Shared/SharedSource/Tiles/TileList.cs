using System.Xml.Linq;
using System.Numerics;

namespace MalignEngine;

public class TileData
{
    public string SceneId { get; private set; }
    public Sprite Icon { get; private set; }

    public TileData(string sceneId, Sprite icon)
    {
        SceneId = sceneId;
        Icon = icon;
    }
}

public class TileList : IAsset
{
    public List<TileData> Tiles { get; private set; }
    public string AssetPath { get; set; }

    private XElement data;

    public TileList(XElement data)
    {
        Tiles = new List<TileData>();

        this.data = data;

        foreach (XElement element in data.Elements())
        {
            string sceneId = element.Attribute("Scene")?.Value;
            string iconPath = element.Attribute("Icon")?.Value;

            if (sceneId == null) { continue; }

            Rectangle? rectangle = null;
            if (element.Attribute("Rectangle") != null)
            {
                string[] rect = element.Attribute("Rectangle")?.Value.Split(',');
                rectangle = new Rectangle(int.Parse(rect[0]), int.Parse(rect[1]), int.Parse(rect[2]), int.Parse(rect[3]));
            }

            Sprite icon = null;
            if (rectangle != null)
            {
                icon = new Sprite(IoCManager.Resolve<AssetSystem>().Load<Texture2D>(iconPath), new Vector2(0.5f, 0.5f), rectangle.Value);
            }
            else
            {
                icon = new Sprite(IoCManager.Resolve<AssetSystem>().Load<Texture2D>(iconPath));
            }

            Tiles.Add(new TileData(sceneId, icon));
        }
    }

    public static TileList Load(string assetPath)
    {
        string fileText = File.ReadAllText(assetPath);

        TileList tileList = new TileList(XElement.Parse(fileText));
        tileList.AssetPath = assetPath;
        return tileList;
    }
}