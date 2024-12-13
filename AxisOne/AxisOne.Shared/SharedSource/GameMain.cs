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

        application.AddSystem(new EntityEventSystem());
        application.AddSystem(new EntityManagerService());
        application.AddSystem(new AssetSystem());
        application.AddSystem(new WindowSystem("Malign Engine", new Vector2(800, 600)));
        application.AddSystem(new GLRenderingSystem());
        application.AddSystem(new InputSystem());
        application.AddSystem(new CameraSystem());
        application.AddSystem(new ParentSystem());
        application.AddSystem(new TransformSystem());
        application.AddSystem(new PhysicsSystem2D());
        application.AddSystem(new SpriteRenderingSystem());
        application.AddSystem(new LightingSystem2D());
        application.AddSystem(new LightingPostProcessingSystem2D());
        application.AddSystem(new AudioSystem());
        application.AddSystem(new FontSystem());
        application.AddSystem(new SceneSystem());
        application.AddSystem(new NetworkingSystem());

        application.AddAllSystems(Assembly.GetExecutingAssembly());

        application.AddSystem(new ImGuiSystem());
        application.AddSystem(new EditorSystem());
        application.AddSystem(new EditorInspectorSystem());
        application.AddSystem(new EditorPerformanceSystem());
        application.AddSystem(new EditorSceneViewSystem());
        application.AddSystem(new EditorAssetViewer());
        application.AddSystem(new EditorConsole());

        application.Run();
    }
}