using System.Numerics;

namespace MalignEngine;

/// <summary>
/// A set of services that can be added to an application.
/// </summary>
public class ServiceSet
{
    public List<Type> Services { get; private set; } = new List<Type>();

    public static ServiceSet DefaultServices
    {
        get
        {
            ServiceSet serviceSet = new ServiceSet(new Type[]
            {
                typeof(EventService),
                typeof(EntityManager),
                typeof(AssetService),
                typeof(WindowService),
                typeof(GLRenderingAPI),
                typeof(Renderer2D),
                typeof(InputSystem),
                typeof(CameraSystem),
                typeof(ParentSystem),
                typeof(PhysicsSystem2D),
                typeof(TransformSystem),
                typeof(SpriteRenderingSystem),
                typeof(LightingSystem2D),
                typeof(LightingPostProcessingSystem2D),
                typeof(AudioSystem),
                typeof(FontSystem),
                typeof(SceneSystem),
                typeof(GUIService)
            });

            return serviceSet;
        }
    }

    public static ServiceSet MinimalServices
    {
        get
        {
            ServiceSet serviceSet = new ServiceSet(new Type[]
            {
                typeof(EntityManager),
                typeof(AssetService),
                typeof(HeadlessUpdateLoop),
                typeof(ParentSystem),
                typeof(TransformSystem),
                typeof(PhysicsSystem2D)
            });

            return serviceSet;
        }
    }

    public static ServiceSet EditorServices
    {
        get
        {
            ServiceSet serviceSet = new ServiceSet(new Type[]
            {
                typeof(ImGuiSystem),
                typeof(EditorSystem),
                typeof(EditorInspectorSystem),
                typeof(EditorPerformanceSystem),
                typeof(EditorSceneViewSystem),
                typeof(EditorAssetViewer),
                typeof(EditorServiceAnalyzer),

            });
            return serviceSet;
        }
    }

    public ServiceSet(Type[] services)
    {
        Services.AddRange(services);
    }

    public void Put(Application application)
    {
        foreach (var service in Services)
        {
            application.ServiceContainer.RegisterAll(service);
        }
    }
}