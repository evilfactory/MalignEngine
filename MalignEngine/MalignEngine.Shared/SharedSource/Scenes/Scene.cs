using Arch.Core;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine;

public class Scene : IAsset
{
    public XElement SceneData { get; private set; }

    internal Func<WorldRef, EntityRef>? customLoadAction { get; private set; }

    public Scene(Func<WorldRef, EntityRef> customLoadAction)
    {
        this.customLoadAction = customLoadAction;
    }

    public Scene(XElement sceneData)
    {
        SceneData = sceneData;
    }

    public void Save(string assetPath)
    {
        SceneData.Save(assetPath);
    }

    public static IAsset Load(string assetPath)
    {
        string fileText = File.ReadAllText(assetPath);

        // Load as XML
        return new Scene(XElement.Parse(fileText));
    }
}