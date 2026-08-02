using System;
using System.Runtime.CompilerServices;

namespace MalignEngine;

public abstract class ComponentStorage
{
    protected bool[] _hasComponent;

    public abstract Type ComponentType { get; }
    public abstract void AddOrSet(Entity entity, IComponent component);
    public abstract void Remove(Entity entity);
    public abstract IComponent GetBoxed(Entity entity);

    public bool Has(Entity entity)
    {
        if (entity.Id >= _hasComponent.Length)
        {
            return false;
        }

        return _hasComponent[entity.Id];
    }

    public static ComponentStorage CreateFromType(Type componentType)
    {
        Type type = typeof(ComponentStorage<>);
        type = type.MakeGenericType(componentType);
        ComponentStorage storage = (ComponentStorage)Activator.CreateInstance(type, [1024])!;
        return storage;
    }
}

public class ComponentStorage<T> : ComponentStorage where T : IComponent
{
    public override Type ComponentType => typeof(T);
    private T[] _components;

    public ComponentStorage(int initialSize = 1024)
    {
        _components = new T[initialSize];
        _hasComponent = new bool[initialSize];
    }

    private void Ensure(Entity entity)
    {
        if (entity.Id >= _components.Length)
        {
            Array.Resize(ref _components, entity.Id * 2);
            Array.Resize(ref _hasComponent, entity.Id * 2);
        }
    }

    public override void AddOrSet(Entity entity, IComponent component)
    {
        Ensure(entity);

        _hasComponent[entity.Id] = true;
        _components[entity.Id] = (T)component;
    }

    public override void Remove(Entity entity)
    {
        _hasComponent[entity.Id] = false;
    }

    public ref T Get(Entity entity)
    {
        return ref _components[entity.Id];
    }

    public override IComponent GetBoxed(Entity entity)
    {
        return _components[entity.Id];
    }
}