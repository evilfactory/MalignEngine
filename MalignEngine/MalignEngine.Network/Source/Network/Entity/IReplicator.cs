namespace MalignEngine.Network;

public record class ReplicationContext(NetworkConnection Connection);

public interface IReplicator
{
    PacketChannel Channel { get; }
    Type ComponentType { get; }
    bool HasChanged(ReplicationContext context, Entity entity);
    void Replicate(ReplicationContext context, Entity entity, IWriteMessage message);
    void Deserialize(ReplicationContext context, Entity entity, IReadMessage message);
}

public abstract class Replicator<T> : IReplicator where T : IComponent
{
    public abstract PacketChannel Channel { get; }

    public Type ComponentType => typeof(T);

    public virtual Type? RequiresComponent { get; } = null;

    protected abstract bool HasChanged(T prevComponent, T currComponent);
    protected abstract void Serialize(Entity entity, IWriteMessage message);
    protected abstract void Deserialize(Entity entity, IReadMessage message);

    private Dictionary<NetworkConnection, T> _states = [];

    public bool HasChanged(ReplicationContext context, Entity entity)
    {
        if (RequiresComponent != null && entity.Has(RequiresComponent))
        {
            return false;
        }

        if (!_states.TryGetValue(context.Connection, out T? value))
        {
            return true;
        }
        else if (HasChanged(value, entity.Get<T>()))
        {
            return true;
        }

        return false;
    }

    public void Replicate(ReplicationContext context, Entity entity, IWriteMessage message)
    {
        Serialize(entity, message);

        _states[context.Connection] = entity.Get<T>();
    }

    public void Deserialize(ReplicationContext context, Entity entity, IReadMessage message) => Deserialize(entity, message);
}