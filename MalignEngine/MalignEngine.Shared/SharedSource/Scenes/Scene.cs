using Arch.Core;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine;

public class Scene : IAsset
{
    public string AssetPath { get; set; }
    public string? SceneId { get; private set; }
    public XElement SceneData { get; private set; }

    internal Func<WorldRef, EntityRef>? customLoadAction { get; private set; }

    public Scene(Func<WorldRef, EntityRef> customLoadAction)
    {
        this.customLoadAction = customLoadAction;
    }

    public Scene(XElement sceneData)
    {
        SceneId = sceneData.Attribute("Identifier")?.Value;
        SceneData = sceneData;
    }

    public void Save(string assetPath)
    {
        SceneData.Save(assetPath);
    }

    public static IAsset Load(string assetPath)
    {
        string fileText = File.ReadAllText(assetPath);

        Scene scene = new Scene(XElement.Parse(fileText));
        scene.AssetPath = assetPath;
        return scene;
    }
}