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

        Editor.Defaults.Editor(application, entityManager);

        application.Initialize();
        eventLoop.Run();
    }

}