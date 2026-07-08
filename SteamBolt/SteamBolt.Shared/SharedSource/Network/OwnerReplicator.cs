using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

public class OwnerReplicator : IReplicator
{
    public PacketChannel Channel => PacketChannel.Reliable;

    public Type ComponentType => typeof(OwnerComponent);

    public void Deserialize(Entity entity, IReadMessage message)
    {
        entity.AddOrSet(new OwnerComponent() { ClientId = new StringClientId(message.ReadString()) });
    }

    public void Serialize(Entity entity, IWriteMessage message)
    {
        message.WriteString(entity.Get<OwnerComponent>().ClientId.StringRepresentation);
    }
}