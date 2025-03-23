using MalignEngine;

namespace MalignEngine.Sample;

public enum GameState
{
    MainMenu,
    BasicRenderingTest,
    CameraTest,
    PhysicsTest,
    LightingTest,
    BasicSceneTest,
    MandelbrotSet,
    ManualCube3D
}


public class Program
{
    public static void Main(string[] args)
    {
        Application application = new Application();
        application.Add(ServiceSet.DefaultServices);
        application.Add(ServiceSet.EditorServices);

        //application.Add<EventService>();
        //application.Add<EntityManager>();
        //application.Add<AssetService>();
        //application.Add<WindowService>();
        //application.Add<GLRenderingAPI>();
        //application.Add<Renderer2D>();

        //typeof(Renderer2D),
        //typeof(InputSystem),
        //typeof(CameraSystem),
        //typeof(ParentSystem),
        //typeof(PhysicsSystem2D),
        //typeof(TransformSystem),
        //typeof(SpriteRenderingSystem),
        //typeof(LightingSystem2D),
        //typeof(LightingPostProcessingSystem2D),
        //typeof(AudioSystem),
        //typeof(FontSystem),
        //typeof(SceneSystem),
        //typeof(GUIService)


        application.Add<SampleInit>();
        application.Add<MainMenu>();
        application.Add<BasicRenderingTest>();
        application.Add<CameraTest>();
        application.Add<PhysicsTest>();
        application.Add<LightingTest>();
        application.Add<BasicSceneTest>();
        application.Add<MandelbrotSet>();
        application.Add<ManualCube3D>();

        application.ScheduleManager.SetMetaData<IAddToUpdateGUIList, MainMenu>(new ScheduleMetaData()
        {
            RunCondition = application.StateManager.Is(GameState.MainMenu)
        });

        application.ScheduleManager.SetMetaData<IDrawGUI, BasicRenderingTest>(new ScheduleMetaData()
        {
            RunCondition = application.StateManager.Is(GameState.BasicRenderingTest)
        });

        application.ScheduleManager.SetMetaData<IUpdate, CameraTest>(new ScheduleMetaData()
        {
            RunCondition = application.StateManager.Is(GameState.CameraTest)
        });

        application.ScheduleManager.SetMetaData<IUpdate, PhysicsTest>(new ScheduleMetaData()
        {
            RunCondition = application.StateManager.Is(GameState.PhysicsTest)
        });

        application.ScheduleManager.SetMetaData<IUpdate, LightingTest>(new ScheduleMetaData()
        {
            RunCondition = application.StateManager.Is(GameState.LightingTest)
        });

        application.ScheduleManager.SetMetaData<IUpdate, BasicSceneTest>(new ScheduleMetaData()
        {
            RunCondition = application.StateManager.Is(GameState.BasicSceneTest)
        });
        
        application.ScheduleManager.SetMetaData<IDrawGUI, MandelbrotSet>(new ScheduleMetaData()
        {
            RunCondition = application.StateManager.Is(GameState.MandelbrotSet)
        });

        application.ScheduleManager.SetMetaData<IDrawGUI, ManualCube3D>(new ScheduleMetaData()
        {
            RunCondition = application.StateManager.Is(GameState.ManualCube3D)
        });

        application.Run();
    }
}