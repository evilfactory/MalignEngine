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

    public void Dispose()
    {
        if (instance is IDisposable disposable)
        {
            disposable.Dispose();
        }

        instance = null;
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

public class ServiceContainer : IDisposable
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
                IEnumerable instances = serviceInterfaces[elementType].Select(x => x.LifeTime.GetInstance(MakeInstanceFactory(x.Implementation)));
                return instances;
            }
            else
            {
                throw new InvalidOperationException($"No service of type {elementType} has been registered");
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
                throw new InvalidOperationException($"No service of type {serviceType} has been registered");
            }
        }
    }

    public T GetInstance<T>()
    {
        object instance = GetInstance(typeof(T));

        if (instance is IEnumerable)
        {
            throw new Exception("Tried to get instance but there's multiple instances");
        }

        return (T)instance;
    }

    public IEnumerable<T> GetInstances<T>()
    {
        IEnumerable instance = (IEnumerable)GetInstance(typeof(IEnumerable<T>));

        return instance.Cast<T>();
    }

    public void Dispose()
    {
        foreach (ILifeTime lifeTime in lifeTimes)
        {
            lifeTime.Dispose();
        }
    }
}