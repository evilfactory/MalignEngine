namespace MalignEngine.Network;

public interface IReplicator
{
    PacketChannel Channel { get; }
    Type ComponentType { get; }
    void Serialize(Entity entity, IWriteMessage message);
    void Deserialize(Entity entity, IReadMessage message);
}