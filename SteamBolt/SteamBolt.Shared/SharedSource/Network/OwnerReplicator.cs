using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

public class OwnerReplicator : Replicator<OwnerComponent>
{
    public override PacketChannel Channel => PacketChannel.Reliable;

    protected override bool HasChanged(OwnerComponent prevComponent, OwnerComponent currComponent)
    {
        return prevComponent.ClientId != currComponent.ClientId;
    }

    protected override void Deserialize(Entity entity, IReadMessage message)
    {
        entity.AddOrSet(new OwnerComponent() { ClientId = new StringClientId(message.ReadString()) });
    }

    protected override void Serialize(Entity entity, IWriteMessage message)
    {
        message.WriteString(entity.Get<OwnerComponent>().ClientId.StringRepresentation);
    }
}