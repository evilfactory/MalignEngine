using Arch.Core;
using Arch.Core.Extensions;

namespace MalignEngine;

public struct EntityRef
{
    public readonly static EntityRef Null = default;

    public int Id => Entity.Entity.Id;
    public int Version => Entity.Version;

    internal EntityReference Entity;
    internal bool Valid = false;

    private WorldRef worldRef;

    internal EntityRef(WorldRef world, Entity entity)
    {
        worldRef = world;
        Entity = entity.Reference();
        Valid = true;
    }

    public void Destroy()
    {
        worldRef.Destroy(this);
    }

    public void Add<T>(in T component) where T : IComponent
    {
        worldRef.AddComponent(this, component);
    }

    public void Add(IComponent component)
    {
        worldRef.AddComponent(this, component);
    }

    public ref T AddOrGet<T>() where T : IComponent, new()
    {
        return ref worldRef.AddOrGetComponent<T>(this);
    }

    public void Remove<T>() where T : IComponent
    {
        worldRef.RemoveComponent<T>(this);
    }

    public void Set<T>(in T component) where T : IComponent
    {
        worldRef.SetComponent(this, component);
    }

    public void Set(object component)
    {
        worldRef.SetComponent(this, component);
    }

    public bool Has<T>() where T : IComponent
    {
        return worldRef.HasComponent<T>(this);
    }

    public ref T Get<T>() where T : IComponent
    {
        return ref worldRef.GetComponent<T>(this);
    }

    public bool TryGet<T>(out T component) where T : IComponent
    {
        return worldRef.TryGetComponent(this, out component);
    }

    public ref T TryGetRef<T>(out bool exists) where T : IComponent
    {
        return ref worldRef.TryGetRefComponent<T>(this, out exists);
    }

    public IComponent[] GetComponents()
    {
        return worldRef.GetComponents(this);
    }

    public bool IsValid()
    {
        return Valid && worldRef.IsValid(this);
    }

    public bool Equals(EntityReference other)
    {
        return Entity.Equals(other.Entity);
    }

    public override bool Equals(object? obj)
    {
        return obj is EntityRef other && Entity.Equals(other.Entity);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Entity.GetHashCode();
        }
    }

    public static bool operator ==(EntityRef left, EntityRef right)
    {
        return left.Entity.Equals(right.Entity);
    }

    public static bool operator !=(EntityRef left, EntityRef right)
    {
        return !left.Entity.Equals(right.Entity);
    }
}