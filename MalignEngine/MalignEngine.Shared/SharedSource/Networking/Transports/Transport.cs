namespace MalignEngine
{
    public enum PacketChannel
    {
        Reliable,
        Unreliable
    }

    public abstract partial class Transport
    {
        public ILogger Logger { get; set; }

        public Action<IReadMessage> OnMessageReceived;

        public Transport() { }

        public abstract void Update();
    }
}
