using System.Collections;
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
    /// <param name="type">An instance</param>
    /// <returns></returns>
    public object GetInstance(Type type);
    /// <summary>
    /// Generic version of GetInstance(Type type)
    /// </summary>
    /// <typeparam name="T">Interface Type</typeparam>
    /// <returns></returns>
    public T GetInstance<T>();
    /// <summary>
    /// If there's more than one implementation type for an interface type, this will return all of them
    /// </summary>
    /// <typeparam name="T">Interface Type</typeparam>
    /// <returns></returns>
    public void Register(Type interfaceType, Type implementationType, ILifeTime lifetime);
    /// <summary>
    /// Registers all interfaces present in the implementation type
    /// </summary>
    /// <param name="implementationType"></param>
    /// <param name="lifetime"></param>
    public void RegisterAll(Type implementationType, ILifeTime lifetime = null);
    /// <summary>
    /// Generic version of Register(Type interfaceType, Type implementationType, ILifeTime lifetime)
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <param name="lifetime"></param>
    public void Register<TInterface, TImplementation>(ILifeTime lifetime);
    /// <summary>
    /// Generic version of RegisterAll(Type implementationType, ILifeTime lifetime = null)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lifetime"></param>
    public void RegisterAll<T>(ILifeTime lifetime);
    /// <summary>
    /// Registers a specific instance to all interface types it implements
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    public void RegisterInstance<T>(T instance) where T : notnull;
    /// <summary>
    /// Injects implementations to all [Dependency] attributes in the instance
    /// </summary>
    /// <param name="obj"></param>
    public void InjectAll(object obj);
}

public class ServiceContainer : IServiceContainer
{
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

    private Dictionary<Type, List<ServiceImplementation>> serviceInterfaces = new Dictionary<Type, List<ServiceImplementation>>();
    private HashSet<ILifeTime> lifeTimes = new HashSet<ILifeTime>();

    public ServiceContainer() { }

    public void InjectAll(object obj)
    {
        var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            Dependency dep = field.GetCustomAttribute<Dependency>();
            if (dep != null)
            {
                object value = GetInstance(field.FieldType);
                if (value == null && !dep.Optional)
                {
                    throw new Exception($"Failed to resolve dependency of type {field.FieldType}");
                }
                field.SetValue(obj, value);
            }
        }
    }

    private Func<object> MakeInstanceFactory(Type type)
    {
        return () =>
        {
            var obj = RuntimeHelpers.GetUninitializedObject(type);

            InjectAll(obj);

            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            ConstructorInfo ctor = constructors.First();

            ParameterInfo[] parameters = ctor.GetParameters();

            object[] passedParameters = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                passedParameters[i] = GetInstance(parameters[i].ParameterType);
            }

            try
            {
                ctor.Invoke(obj, passedParameters);
            }
            catch(TargetInvocationException exception)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            }

            InjectAll(obj);

            return obj;
        };
    }

    public void RegisterInstance<T>(T instance) where T : notnull
    {
        RegisterAll(typeof(T), new SingletonLifeTime(instance));
    }

    public void Register(Type interfaceType, Type implementationType, ILifeTime lifetime)
    {
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

    public void Register<TInterface, TImplementation>(ILifeTime lifetime)
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

    public object GetInstance(Type serviceType)
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

                return array;
            }
            else
            {
                return Array.CreateInstance(elementType, 0);
            }
        }
        else
        {
            if (serviceInterfaces.ContainsKey(serviceType))
            {
                return serviceInterfaces[serviceType].First().LifeTime.GetInstance(MakeInstanceFactory(serviceInterfaces[serviceType].First().Implementation));
            }
            else
            {
                return null;
                //throw new InvalidOperationException($"No service of type {serviceType} has been registered");
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
}