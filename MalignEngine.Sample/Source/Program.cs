using MalignEngine;

namespace MalignEngine.Sample;

public enum GameState
{
    MainMenu,
    BasicRenderingTest,
    CameraTest,
    PhysicsTest,
    LightingTest
}


public class Program
{
    public static void Main(string[] args)
    {
        Application application = new Application();

        application.Add(ServiceSet.DefaultServices);
        application.Add(ServiceSet.EditorServices);

        application.Add(new SampleInit());
        application.Add(new MainMenu());
        application.Add(new BasicRenderingTest());
        application.Add(new CameraTest());
        application.Add(new PhysicsTest());
        application.Add(new LightingTest());

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

        application.Run();
    }
}