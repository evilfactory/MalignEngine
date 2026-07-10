using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
namespace MalignEngine;

public interface ILifeTime : IDisposable
{
    object GetInstance(Func<object> instanceFactory);
}

public class SingletonLifeTime : ILifeTime
{
    private object? instance;

    public SingletonLifeTime() { }
    public SingletonLifeTime(object instance)
    {
        this.instance = instance;
    }

    public void Dispose()
    {
        if (instance is not IServiceContainer && instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public object GetInstance(Func<object> instanceFactory)
    {
        if (instance == null)
        {
            instance = instanceFactory();
        }

        return instance;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class Dependency : Attribute
{
    public bool Optional { get; private set; } = false;

    public Dependency() { }
    public Dependency(bool optional)
    {
        Optional = optional;
    }
}

public interface IServiceContainer : IDisposable
{
    /// <summary>
    /// Gets a specific instance by supplying the interface type
    /// </summary>
    public object GetInstance(Type type);
    /// <summary>
    /// Generic version of <see cref="GetInstance(Type)"/>
    /// </summary>
    public T GetInstance<T>();
    /// <summary>
    /// <see cref="GetInstance"/> that is allowed to fail.
    /// </summary>
    public bool TryGetInstance(Type type, [NotNullWhen(returnValue: true)] out object? instance);
    /// <summary>
    /// Generic version of <see cref="TryGetInstance(Type, out object?)"/>
    /// </summary>
    public bool TryGetInstance<T>([NotNullWhen(returnValue: true)] out T? instance);
    /// <summary>
    /// If there's more than one implementation type for an interface type, this will return all of them
    /// </summary>
    public void Register(Type interfaceType, Type implementationType, ILifeTime? lifetime = null);
    /// <summary>
    /// Registers all interfaces present in the implementation type
    /// </summary>
    public void RegisterAll(Type implementationType, ILifeTime? lifetime = null);
    /// <summary>
    /// Generic version of Register(Type interfaceType, Type implementationType, ILifeTime lifetime)
    /// </summary>
    public void Register<TInterface, TImplementation>(ILifeTime? lifetime = null);
    /// <summary>
    /// Generic version of RegisterAll(Type implementationType, ILifeTime lifetime = null)
    /// </summary>
    public void RegisterAll<T>(ILifeTime? lifetime = null);
    /// <summary>
    /// Injects implementations to all [Dependency] attributes in the instance
    /// </summary>
    public void InjectAll(object obj);
}

public class ServiceContainer : IServiceContainer
{
    private readonly IServiceContainer? parent;

    private class ServiceImplementation
    {
        public Type Implementation { get; }
        public ILifeTime LifeTime { get; }

        public ServiceImplementation(Type implementation, ILifeTime lifeTime)
        {
            Implementation = implementation;
            LifeTime = lifeTime;
        }
    }

    private readonly Dictionary<Type, List<ServiceImplementation>> serviceInterfaces = new Dictionary<Type, List<ServiceImplementation>>();
    private readonly HashSet<ILifeTime> lifeTimes = new HashSet<ILifeTime>();

    public ServiceContainer(IServiceContainer? parent = null)
    {
        this.parent = parent;

        Register<IServiceContainer, ServiceContainer>(new SingletonLifeTime(this));
    }

    public void InjectAll(object obj)
    {
        var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            Dependency? dep = field.GetCustomAttribute<Dependency>();
            if (dep != null)
            {
                if (!TryGetInstance(field.FieldType, out object? instance) && !dep.Optional)
                {
                    throw new Exception($"Failed to resolve dependency of type {field.FieldType}");
                }
                field.SetValue(obj, instance);
            }
        }
    }

    private Func<object> MakeInstanceFactory(Type type)
    {
        return () =>
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            ConstructorInfo ctor = constructors.First();

            ParameterInfo[] parameters = ctor.GetParameters();

            object[] passedParameters = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (TryGetInstance(parameters[i].ParameterType, out object? instance))
                {
                    passedParameters[i] = instance;
                }
                else if (!parameters[i].HasDefaultValue)
                {
                    throw new Exception($"Failed to find implementation for {parameters[i].ParameterType.Name}");
                }
            }

            object obj = null;

            obj = Activator.CreateInstance(type, passedParameters)!;

            InjectAll(obj);

            return obj;
        };
    }

    public void Register(Type interfaceType, Type implementationType, ILifeTime? lifetime = null)
    {
        lifetime = lifetime ?? new SingletonLifeTime();

        if (!serviceInterfaces.ContainsKey(interfaceType))
        {
            serviceInterfaces[interfaceType] = new List<ServiceImplementation>();
        }

        serviceInterfaces[interfaceType].Add(new ServiceImplementation(implementationType, lifetime));
        
        if (!lifeTimes.Contains(lifetime))
        {
            lifeTimes.Add(lifetime);
        }
    }

    public void Register<TInterface, TImplementation>(ILifeTime? lifetime = null)
    {
        Register(typeof(TInterface), typeof(TImplementation), lifetime);
    }

    public void RegisterAll(Type implementationType, ILifeTime? lifetime = null)
    {
        lifetime = lifetime ?? new SingletonLifeTime();

        Type[] interfaces = implementationType.GetInterfaces();

        foreach (Type inter in interfaces)
        {
            Register(inter, implementationType, lifetime);
        }

        Register(implementationType, implementationType, lifetime);
    }

    public void RegisterAll<T>(ILifeTime? lifetime = null) => RegisterAll(typeof(T), lifetime);

    public bool TryGetInstance(Type serviceType, [NotNullWhen(true)] out object? instance)
    {
        if (serviceType.IsGenericType && serviceType.IsAssignableTo(typeof(IEnumerable)))
        {
            Type elementType = serviceType.GetGenericArguments()[0];

            if (serviceInterfaces.ContainsKey(elementType))
            {
                List<object> instances = serviceInterfaces[elementType].Select(x => x.LifeTime.GetInstance(MakeInstanceFactory(x.Implementation))).ToList();

                Array array = Array.CreateInstance(elementType, instances.Count);
                
                for (int i = 0; i < instances.Count; i++)
                {
                    array.SetValue(instances[i], i);
                }

                instance = array;
                return true;
            }
            else
            {
                if (parent != null)
                {
                    instance = parent.GetInstance(serviceType);
                    return true;
                }

                instance = Array.CreateInstance(elementType, 0);
                return true;
            }
        }
        else
        {
            if (serviceInterfaces.ContainsKey(serviceType))
            {
                instance = serviceInterfaces[serviceType].First().LifeTime.GetInstance(MakeInstanceFactory(serviceInterfaces[serviceType].First().Implementation));
                return true;
            }
            else
            {
                if (parent != null)
                {
                    if (parent.TryGetInstance(serviceType, out instance))
                    {
                        return true;
                    }
                }

                instance = null;
                return false;
            }
        }
    }

    public T GetInstance<T>()
    {
        return (T)GetInstance(typeof(T));
    }

    public void Dispose()
    {
        foreach (ILifeTime lifeTime in lifeTimes.Reverse())
        {
            lifeTime.Dispose();
        }
    }

    public object GetInstance(Type type)
    {
        if (TryGetInstance(type, out object? instance))
        {
            return instance;
        }

        throw new InvalidOperationException($"No service of type {type} has been registered");
    }

    public bool TryGetInstance<T>([NotNullWhen(true)] out T? instance)
    {
        if (TryGetInstance(typeof(T), out object? result))
        {
            instance = (T)result;
        }

        instance = default;
        return false;
    }
}