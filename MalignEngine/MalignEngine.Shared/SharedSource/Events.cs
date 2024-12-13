namespace MalignEngine
{
    public interface IEvent { }

    public interface IApplicationRun : IEvent
    {
        public void OnApplicationRun();
    }
    public interface IInit : IEvent
    {
        public void OnInitialize();
    }
    public interface IPreUpdate : IEvent
    {
        public void OnPreUpdate(float deltaTime);
    }
    public interface IUpdate : IEvent
    {
        public void OnUpdate(float deltaTime);
    }
    public interface IPostUpdate : IEvent
    {
        public void OnPostUpdate(float deltaTime);
    }
    public interface IDraw : IEvent
    {
        public void OnDraw(float deltaTime);
    }
    public interface IWindowDraw : IEvent
    {
        public void OnWindowDraw(float deltaTime);
    }
    public interface IDrawGUI : IEvent
    {
        public void OnDrawGUI(float deltaTime);
    }
    public interface IPreDrawGUI : IEvent
    {
        public void OnPreDrawGUI(float deltaTime);
    }
    public interface IPostDrawGUI : IEvent
    {
        public void OnPostDrawGUI(float deltaTime);
    }
}