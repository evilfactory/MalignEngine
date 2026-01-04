using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public interface IWorld
{
    public Entity CreateEntity();
    public void DestroyImmediate(Entity entity);
    public bool IsAlive(Entity entity);
    public void AddOrSetComponent(Entity entity, IComponent component);
    public bool RemoveComponent(Entity entity, Type type);
    public bool RemoveComponent<T>(Entity entity) where T : IComponent;
    public bool HasComponent(Entity entity, Type type);
    public bool HasComponent<T>(Entity entity) where T : IComponent;
    public IComponent GetComponent(Entity entity, Type type);
    public ref T GetComponent<T>(Entity entity) where T : IComponent;
    public ref T GetOrAddComponent<T>(Entity entity) where T : IComponent, new();
    public bool TryGetComponent(Entity entity, Type type, [NotNullWhen(returnValue: true)] out IComponent? component);
    public bool TryGetComponent<T>(Entity entity, [NotNullWhen(returnValue: true)] out ComponentRef<T> component) where T : IComponent;
    public IEnumerable<IComponent> GetComponents(Entity entity);
    public void Query(Query query, Action<Entity> forEach);
}

public sealed class World : IWorld
{
    private readonly List<int> _versions = new();
    private readonly Stack<int> _freeIds = new();

    private readonly ConcurrentDictionary<Type, IComponentStorage> _storages = new();
    public World() { }

    #region Entity

    public Entity CreateEntity()
    {
        int id;
        if (_freeIds.Count > 0)
        {
            id = _freeIds.Pop();
        }
        else
        {
            id = _versions.Count;
            _versions.Add(1);
        }

        return new Entity(this, id, _versions[(int)id]);
    }

    public bool IsAlive(Entity entity)
    {
        return entity.Id >= 0 &&
            entity.Id < _versions.Count &&
            _versions[(int)entity.Id] == entity.Version;
    }

    public void DestroyImmediate(Entity entity)
    {
        if (!IsAlive(entity))
        {
            return;
        }

        _versions[(int)entity.Id]++;
        _freeIds.Push(entity.Id);

        foreach (var storage in _storages.Values)
        {
            if (storage.Has(entity))
            {
                storage.Remove(entity);
            }
        }
    }

    #endregion

    #region Component

    private IComponentStorage GetComponentStorage(Type type)
    {
        if (!_storages.ContainsKey(type))
        {
            _storages.TryAdd(type, IComponentStorage.CreateFromType(type));
        }

        return _storages[type];
    }

    public void AddOrSetComponent(Entity entity, IComponent component)
    {
        if (!IsAlive(entity))
        {
            throw new InvalidOperationException($"{entity} is not alive");
        }

        IComponentStorage storage = GetComponentStorage(component.GetType());
        storage.AddOrSet(entity, component);
    }

    public bool RemoveComponent(Entity entity, Type type)
    {
        if (!IsAlive(entity))
        {
            throw new InvalidOperationException($"{entity} is not alive");
        }

        IComponentStorage storage = GetComponentStorage(type);
        if (storage.Has(entity))
        {
            storage.Remove(entity);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool RemoveComponent<T>(Entity entity) where T : IComponent
        => RemoveComponent(entity, typeof(T));

    public bool HasComponent(Entity entity, Type type)
    {
        if (!IsAlive(entity))
        {
            throw new InvalidOperationException($"{entity} is not alive");
        }

        IComponentStorage storage = GetComponentStorage(type);
        return storage.Has(entity);
    }

    public bool HasComponent<T>(Entity entity) where T : IComponent
        => HasComponent(entity, typeof(T));

    public IComponent GetComponent(Entity entity, Type type)
    {
        if (!IsAlive(entity))
        {
            throw new InvalidOperationException($"{entity} is not alive");
        }

        IComponentStorage storage = GetComponentStorage(type);

        if (!storage.Has(entity))
        {
            throw new InvalidOperationException($"{entity} doesn't have component type {type.Name}");
        }

        return storage.GetBoxed(entity);
    }

    public ref T GetComponent<T>(Entity entity) where T : IComponent
    {
        if (!IsAlive(entity))
        {
            throw new InvalidOperationException($"{entity} is not alive");
        }

        Type type = typeof(T);
        IComponentStorage storage = GetComponentStorage(type);

        if (!storage.Has(entity))
        {
            throw new InvalidOperationException($"{entity} doesn't have component type {type.Name}");
        }

        return ref ((IComponentStorage<T>)storage).Get(entity);
    }

    public ref T GetOrAddComponent<T>(Entity entity) where T : IComponent, new()
    {
        if (!IsAlive(entity))
        {
            throw new InvalidOperationException($"{entity} is not alive");
        }

        Type type = typeof(T);
        IComponentStorage storage = GetComponentStorage(type);

        if (!storage.Has(entity))
        {
            storage.AddOrSet(entity, new T());
        }

        return ref ((IComponentStorage<T>)storage).Get(entity);
    }

    public bool TryGetComponent(Entity entity, Type type, [NotNullWhen(returnValue: true)] out IComponent? component)
    {
        component = default;

        if (!IsAlive(entity))
        {
            return false;
        }

        IComponentStorage storage = GetComponentStorage(type);

        if (!storage.Has(entity))
        {
            return false;
        }
        else
        {
            component = storage.GetBoxed(entity);
            return true;
        }
    }

    public bool TryGetComponent<T>(Entity entity, [NotNullWhen(returnValue: true)] out ComponentRef<T> component) where T : IComponent
    {
        component = default;

        if (!IsAlive(entity))
        {
            return false;
        }

        IComponentStorage storage = GetComponentStorage(typeof(T));

        if (!storage.Has(entity))
        {
            return false;
        }
        else
        {
            component = new ComponentRef<T>(ref ((IComponentStorage<T>)storage).Get(entity));
            return true;
        }
    }

    public IEnumerable<IComponent> GetComponents(Entity entity)
    {
        if (!IsAlive(entity))
        {
            throw new InvalidOperationException($"{entity} is not alive");
        }

        foreach (var storage in _storages.Values)
        {
            if (storage.Has(entity))
            {
                yield return storage.GetBoxed(entity);
            }
        }
    }

    public void Query(Query query, Action<Entity> forEach)
    {
        if (query.All.Count() == 0)
        {
            return;
        }

        for (int id = 0; id < _versions.Count; id++)
        {
            var entity = new Entity(this, id, _versions[id]);

            if (!Matches(entity, query))
            {
                continue;
            }

            forEach(entity);
        }
    }

    private bool Matches(Entity entity, Query query)
    {
        foreach (var type in query.All)
        {
            if (!GetComponentStorage(type).Has(entity))
            {
                return false;
            }
        }

        foreach (var type in query.None)
        {
            if (GetComponentStorage(type).Has(entity))
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}