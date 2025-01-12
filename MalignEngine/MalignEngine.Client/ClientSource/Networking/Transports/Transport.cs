using System.Net;

namespace MalignEngine
{
    partial class Transport
    {
        public Action OnConnected;
        public Action OnDisconnected;

        public abstract void SendToServer(IWriteMessage message, PacketChannel packetChannel = PacketChannel.Reliable);
        public abstract void Connect(IPEndPoint endpoint);
        public abstract void Disconnect();

    }
}