using MalignEngine.Network;
using System.Numerics;

namespace MalignEngine;

public struct NetworkTransform : IComponent { }

public class TransformReplicator : Replicator<Transform>
{
    public override PacketChannel Channel => PacketChannel.Reliable;

    public override Type? RequiresComponent => typeof(NetworkTransform);

    protected override void Serialize(Entity entity, IWriteMessage message)
    {
        Transform transform = entity.Get<Transform>();

        message.WriteSingle(transform.Position.X);
        message.WriteSingle(transform.Position.Y);
        message.WriteSingle(transform.Position.Z);
    }

    protected override void Deserialize(Entity entity, IReadMessage message)
    {
        ref Transform transform = ref entity.Get<Transform>();

        transform.Position = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
    }

    protected override bool HasChanged(Transform prevComponent, Transform currComponent)
    {
        return Vector3.DistanceSquared(prevComponent.Position, currComponent.Position) > 10.0f;
    }
}