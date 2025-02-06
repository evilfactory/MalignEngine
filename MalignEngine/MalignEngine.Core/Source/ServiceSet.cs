using System.Numerics;

namespace MalignEngine;

/// <summary>
/// A set of services that can be added to an application.
/// </summary>
public class ServiceSet
{
    public List<IService> Services { get; private set; } = new List<IService>();
    public ScheduleMetaData? ScheduleMetaData { get; set; }

    public static ServiceSet DefaultServices
    {
        get
        {
            ServiceSet serviceSet = new ServiceSet(new IService[]
            {
                new EntityManagerService(),
                new EntityEventSystem(),
                new AssetService(),
                new WindowSystem("Malign Engine", new Vector2(800, 600)),
                new GLRenderingSystem(),
                new InputSystem(),
                new CameraSystem(),
                new ParentSystem(),
                new TransformSystem(),
                new PhysicsSystem2D(),
                new SpriteRenderingSystem(),
                new LightingSystem2D(),
                new LightingPostProcessingSystem2D(),
                new AudioSystem(),
                new FontSystem(),
                new SceneSystem(),
                new GUIService(),
            });

            return serviceSet;
        }
    }

    public static ServiceSet MinimalServices
    {
        get
        {
            ServiceSet serviceSet = new ServiceSet(new IService[]
            {
                new EntityManagerService(),
                new EntityEventSystem(),
                new AssetService(),
                new HeadlessUpdateLoop(),
                new ParentSystem(),
                new TransformSystem(),
                new PhysicsSystem2D()
            });

            return serviceSet;
        }
    }

    public static ServiceSet EditorServices
    {
        get
        {
            ServiceSet serviceSet = new ServiceSet(new IService[]
            {
                new ImGuiSystem(),
                new EditorSystem(),
                new EditorInspectorSystem(),
                new EditorPerformanceSystem(),
                new EditorSceneViewSystem(),
                new EditorAssetViewer(),
                new EditorServiceAnalyzer(),

            });
            return serviceSet;
        }
    }

    public ServiceSet()
    {
    }

    public ServiceSet(IService[] services)
    {
        Services.AddRange(services);
    }

    public void Add(IService service)
    {
        Services.Add(service);
    }

    public void Add(params IService[] services)
    {
        Services.AddRange(services);
    }

    public void Put(Application application)
    {
        foreach (var service in Services)
        {
            application.Add(service, ScheduleMetaData);
        }
    }
}