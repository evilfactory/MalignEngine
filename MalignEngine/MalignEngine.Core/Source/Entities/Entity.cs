using System.Diagnostics.CodeAnalysis;

namespace MalignEngine;

public interface IWorldEntity
{
    ref T Get<T>() where T : IComponent;
    bool TryGet<T>([NotNullWhen(returnValue: true)] out ComponentRef<T> component) where T : IComponent;
    void AddOrSet(IComponent component);
    IEnumerable<IComponent> GetComponents();
    bool Has<T>() where T : IComponent;
    void Remove<T>() where T : IComponent;
    bool IsAlive();
}

public struct Entity : IWorldEntity
{
    public readonly static Entity Null = default;
    public int Id { get; private set; }
    public int Version { get; private set; }
    public World World { get; private set; }

    internal Entity(World world, int id, int version)
    {
        Id = id;
        Version = version;
        World = world;
    }

    public void AddOrSet(IComponent component)
        => World.AddOrSetComponent(this, component);

    public ref T Get<T>() where T : IComponent
    {
        return ref World.GetComponent<T>(this);
    }

    public bool TryGet<T>([NotNullWhen(returnValue: true)] out ComponentRef<T> component) where T : IComponent
    {
        return World.TryGetComponent(this, out component);
    }

    public IEnumerable<IComponent> GetComponents() => World.GetComponents(this);

    public bool Has<T>() where T : IComponent
        => World.HasComponent<T>(this);

    public void Remove<T>() where T : IComponent
        => World.RemoveComponent<T>(this);

    public bool IsAlive() => World != null && World.IsAlive(this);

    public override string ToString() => $"Entity({Id})";

    public override bool Equals(object? obj)
    {
        return obj is Entity other && other.Id == Id && Version == other.Version;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Version);
    }

    public static bool operator ==(Entity left, Entity right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }
}
