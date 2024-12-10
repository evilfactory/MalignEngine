namespace MalignEngine
{
    public enum PacketChannel
    {
        Reliable,
        Unreliable
    }

    public abstract partial class Transport
    {
        protected ILogger Logger;

        public Action<IReadMessage> OnMessageReceived;

        public Transport(ILogger logger)
        {
            Logger = logger;
        }

        public abstract void Update();
    }
}
