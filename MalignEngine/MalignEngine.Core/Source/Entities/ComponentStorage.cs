using System;
using System.Runtime.CompilerServices;

namespace MalignEngine;

public interface IComponentStorage
{
    Type ComponentType { get; }
    void AddOrSet(Entity entity, IComponent component);
    void Remove(Entity entity);
    bool Has(Entity entity);
    IComponent GetBoxed(Entity entity);

    public static IComponentStorage CreateFromType(Type componentType)
    {
        Type type = typeof(ComponentStorage<>);
        type = type.MakeGenericType(componentType);
        IComponentStorage storage = (IComponentStorage)Activator.CreateInstance(type, [1024])!;
        return storage;
    }
}

public interface IComponentStorage<T> : IComponentStorage
{
    ref T Get(Entity entity);
}

public class ComponentStorage<T> : IComponentStorage<T> where T : IComponent
{
    public Type ComponentType => typeof(T);
    private T[] _components;
    private bool[] _hasComponent;

    public ComponentStorage(int initialSize = 1024)
    {
        _components = new T[initialSize];
        _hasComponent = new bool[initialSize];
    }

    private void Ensure(Entity entity)
    {
        if (entity.Id >= _components.Length)
        {
            Array.Resize(ref _components, _components.Length * 2);
            Array.Resize(ref _hasComponent, _hasComponent.Length * 2);
        }
    }

    public bool Has(Entity entity)
    {
        if (entity.Id >= _hasComponent.Length)
        {
            return false;
        }

        return _hasComponent[entity.Id];
    }

    public void AddOrSet(Entity entity, IComponent component)
    {
        Ensure(entity);

        _hasComponent[entity.Id] = true;
        _components[entity.Id] = (T)component;
    }

    public void Remove(Entity entity)
    {
        _hasComponent[entity.Id] = false;
    }

    public ref T Get(Entity entity)
    {
        return ref _components[entity.Id];
    }

    public IComponent GetBoxed(Entity entity)
    {
        return _components[entity.Id];
    }
}