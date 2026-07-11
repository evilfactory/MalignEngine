using MalignEngine;
using MalignEngine.Editor;

namespace MalignEngine.Samples.Cubes;

class Program
{
    public static void Main(string[] args)
    {
        Application application = new DesktopApplication();
        application.ServiceContainer.RegisterAll<WindowService>();
        application.ServiceContainer.RegisterAll<GLRenderingAPI>();
        application.ServiceContainer.RegisterAll<InputService>();
        application.ServiceContainer.RegisterAll<Renderer2D>();
        application.ServiceContainer.RegisterAll<Cubes>();
        application.ServiceContainer.RegisterAll<ImGuiSystem>();
        application.ServiceContainer.RegisterAll<PerformanceProfiler>();
        application.ServiceContainer.RegisterAll<AssetService>();
        application.ServiceContainer.RegisterAll<TextureAssetLoader>();
        application.ServiceContainer.RegisterAll<ShaderAssetLoader>();
        application.ServiceContainer.RegisterAll<EditorSystem>();
        application.ServiceContainer.RegisterAll<EditorPerformanceSystem>();

        EventLoop eventLoop = new EventLoop(
            application.ServiceContainer.GetInstance<IScheduleManager>(),
            new ExecutionPipeline()
                .Stage<IPreUpdate>((s, c) => s.OnPreUpdate((float)c.DeltaTime))
                .Stage<IUpdate>((s, c) => s.OnUpdate((float)c.DeltaTime))
                .Stage<IPostUpdate>((s, c) => s.OnPostUpdate((float)c.DeltaTime))
                .Stage<ICommitWorldChanges>((s, c) => s.CommitWorldChanges()),
            new ExecutionPipeline()
                .Stage<IBeginFrame>((s, c) => s.BeginFrame())
                .Stage<IPreDraw>((s, c) => s.OnPreDraw((float)c.DeltaTime))
                .Stage<IDraw>((s, c) => s.OnDraw((float)c.DeltaTime))
                .Stage<IPostDraw>((s, c) => s.OnPostDraw((float)c.DeltaTime))
                .Stage<IEndFrame>((s, c) => s.EndFrame())
            );

        application.ServiceContainer.Register<IEventLoop, EventLoop>(new SingletonLifeTime(eventLoop));

        application.Initialize();
        eventLoop.Run();
    }

}