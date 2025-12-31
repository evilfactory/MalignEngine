using MalignEngine;

namespace MalignEngine
{   
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
    public interface IPreDraw : ISchedule
    {
        public void OnPreDraw(float deltaTime);
    }
    public interface IDraw : ISchedule
    {
        public void OnDraw(float deltaTime);
    }
    public interface IPostDraw : ISchedule
    {
        public void OnPostDraw(float deltaTime);
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

    public interface IApplicationClosing : ISchedule
    {
        public void OnApplicationClosing();

    }
}

public class BeforeEndFrame : Stage
{
    public override float Priority => 0.99f;
}