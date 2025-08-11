namespace MalignEngine
{    public interface IApplicationRun : ISchedule
    {
        public void OnApplicationRun();
    }
    public interface IInit : ISchedule
    {
        public void OnInitialize();
    }
    public interface IPreUpdate : ISchedule
    {
        public void OnPreUpdate(float deltaTime);
    }
    public interface IUpdate : ISchedule
    {
        public void OnUpdate(float deltaTime);
    }
    public interface IPostUpdate : ISchedule
    {
        public void OnPostUpdate(float deltaTime);
    }
    public interface IDraw : ISchedule
    {
        public void OnDraw(float deltaTime);
    }
    public interface IDrawGUI : ISchedule
    {
        public void OnDrawGUI(float deltaTime);
    }
    public interface IPreDrawGUI : ISchedule
    {
        public void OnPreDrawGUI(float deltaTime);
    }
    public interface IPostDrawGUI : ISchedule
    {
        public void OnPostDrawGUI(float deltaTime);
    }
}