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

public class TileList : XmlAsset<TileList>
{
    public List<TileData> Tiles { get; private set; }

    public TileList()
    {
        Tiles = new List<TileData>();
    }

    public override void Load(XElement root)
    {
        foreach (XElement element in root.Elements())
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
                icon = new Sprite(IoCManager.Resolve<AssetService>().FromFile<Texture2D>(iconPath), new Vector2(0.5f, 0.5f), rectangle.Value);
            }
            else
            {
                icon = new Sprite(IoCManager.Resolve<AssetService>().FromFile<Texture2D>(iconPath));
            }

            Tiles.Add(new TileData(sceneId, icon));
        }
    }

    public override void Save(XElement element)
    {
        throw new NotImplementedException();
    }
}