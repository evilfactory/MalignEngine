namespace MalignEngine
{
    partial class Transport
    {
        public Action<NetworkConnection> OnClientConnected;
        public Action<NetworkConnection> OnClientDisconnected;

        public abstract void SendToClient(IWriteMessage message, NetworkConnection connection, PacketChannel packetChannel = PacketChannel.Reliable);
        public abstract void Listen(int port);
        public abstract void Shutdown();

    }
}