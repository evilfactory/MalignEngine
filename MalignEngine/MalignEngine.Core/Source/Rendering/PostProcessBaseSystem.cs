namespace MalignEngine
{    public abstract class PostProcessBaseSystem : BaseSystem
    {
        protected PostProcessBaseSystem(IServiceContainer serviceContainer) : base(serviceContainer)
        {
        }

        public abstract void Process(IFrameBufferResource source);
    }
}