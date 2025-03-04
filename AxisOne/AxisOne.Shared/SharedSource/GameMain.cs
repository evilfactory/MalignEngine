using MalignEngine;
using Arch.Core;
using System.Numerics;
using Arch.Core.Extensions;
using System.Net;
using System.Reflection;

namespace AxisOne;

class GameMain
{
    public GameMain()
    {
        Application application = new Application();

        application.Add(new EventService());
        application.Add(new EntityManagerService());
        application.Add(new AssetService());
        application.Add(new WindowSystem("Malign Engine", new Vector2(800, 600)));
        application.Add(new GLRenderingSystem());
        application.Add(new InputSystem());
        application.Add(new CameraSystem());
        application.Add(new ParentSystem());
        application.Add(new TransformSystem());
        application.Add(new PhysicsSystem2D());
        application.Add(new SpriteRenderingSystem());
        application.Add(new LightingSystem2D());
        application.Add(new LightingPostProcessingSystem2D());
        application.Add(new AudioSystem());
        application.Add(new FontSystem());
        application.Add(new SceneSystem());
        application.Add(new NetworkingSystem());
        application.Add(new TileSystem());

        application.Add(Assembly.GetExecutingAssembly());

        application.Add(new ImGuiSystem());
        application.Add(new EditorSystem());
        application.Add(new EditorInspectorSystem());
        application.Add(new EditorPerformanceSystem());
        application.Add(new EditorSceneViewSystem());
        application.Add(new EditorAssetViewer());
        application.Add(new EditorConsole());
        application.Add(new EditorNetworking());
        application.Add(new EditorTile());

        application.Run();
    }
}