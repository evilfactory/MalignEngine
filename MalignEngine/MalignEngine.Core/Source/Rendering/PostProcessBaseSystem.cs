namespace MalignEngine
{    public abstract class PostProcessBaseSystem : BaseSystem
    {
        protected PostProcessBaseSystem(ILoggerService loggerService, IScheduleManager scheduleManager) : base(loggerService, scheduleManager)
        {
        }

        public abstract void Process(IFrameBufferResource source);
    }
}