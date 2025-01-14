namespace MalignEngine;

partial class Transport
{
    public Action<NetworkConnection> OnClientConnected;
    public Action<NetworkConnection, DisconnectReason> OnClientDisconnected;

    public abstract void SendToClient(IWriteMessage message, NetworkConnection connection, PacketChannel packetChannel = PacketChannel.Reliable);
    public abstract void Listen(int port);
    public abstract void Shutdown();
    public abstract void DisconnectClient(NetworkConnection connection, DisconnectReason reason);

}