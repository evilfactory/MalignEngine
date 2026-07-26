using MalignEngine.Editor;
using System.Reflection;

namespace MalignEngine.Experimentation;

class Program
{
    public static void Main(string[] args)
    {
        Application application = new DesktopApplication();

        Defaults.Essentials(application);
        EntityManager entityManager = Defaults.Entity(application);
        EventLoop eventLoop = Defaults.EventLoop(application);

        entityManager.WorldContainer.RegisterAssembly(Assembly.GetExecutingAssembly(), [typeof(EntitySystem)], []);

        application.ServiceContainer.RegisterAll<ImGuiSystem>();
        application.ServiceContainer.RegisterAll<EditorSystem>();
        application.ServiceContainer.RegisterAll<EditorConsole>();
        application.ServiceContainer.RegisterAll<EditorPerformanceSystem>();
        application.ServiceContainer.RegisterAll<EditorAssetViewer>();
        entityManager.WorldContainer.RegisterAll<EditorSceneViewSystem>();
        entityManager.WorldContainer.RegisterAll<EditorInspectorSystem>();

        application.Initialize();
        eventLoop.Run();
    }

}