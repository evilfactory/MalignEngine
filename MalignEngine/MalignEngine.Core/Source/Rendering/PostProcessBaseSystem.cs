namespace MalignEngine
{    public abstract class PostProcessBaseSystem : BaseSystem
    {
        public abstract void Process(IFrameBufferResource source);
    }
}