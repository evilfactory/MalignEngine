using MalignEngine;
using System.Numerics;

namespace Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Application application = new Application();

            application.AddSystem(new WorldSystem());
            application.AddSystem(new EntityEventSystem());
            application.AddSystem(new AssetSystem());
            application.AddSystem(new WindowSystem("Malign Engine", new Vector2(800, 600)));
            application.AddSystem(new GLRenderingSystem());
            application.AddSystem(new InputSystem());
            application.AddSystem(new ImGuiSystem());
            application.AddSystem(new CameraSystem());
            application.AddSystem(new ParentSystem());
            application.AddSystem(new TransformSystem());
            application.AddSystem(new Physics2DSystem());
            application.AddSystem(new SpriteRenderingSystem());
            application.AddSystem(new LightingSystem2D());
            application.AddSystem(new LightingPostProcessingSystem2D());
            application.AddSystem(new EditorSystem());
            application.AddSystem(new EditorInspectorSystem());
            application.AddSystem(new EditorPerformanceSystem());
            application.AddSystem(new EditorSceneViewSystem());

            application.AddSystem(new GameMain());

            application.Run();
        }
    }
}