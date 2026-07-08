using MalignEngine;

namespace MalignEngine.Network;

public interface INetSerializable { }

public interface INetworkSerializable
{
    void Deserialize(IReadMessage message);
    void Serialize(IWriteMessage message);
}

public abstract class NetMessage : INetworkSerializable
{
    public string MsgName { get; }
    public PacketChannel Channel { get; set; } = PacketChannel.Reliable;

    protected NetMessage()
    {
        MsgName = GetType().Name;
    }

    public abstract void Deserialize(IReadMessage message);
    public abstract void Serialize(IWriteMessage message);
}