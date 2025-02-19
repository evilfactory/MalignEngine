using System.Reflection;

namespace MalignEngine
{
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

    public static class IoCManager
    {
        private static readonly Dictionary<Type, object> instances = new Dictionary<Type, object>();

        public static void Register(object instance)
        {
            instances[instance.GetType()] = instance;
        }

        public static object Resolve(Type type)
        {
            // Include types that inherit from type or implement the interface
            return instances.Values.FirstOrDefault(x => x.GetType() == type || x.GetType().IsSubclassOf(type) || type.IsAssignableFrom(x.GetType()));
        }

        public static T Resolve<T>()
        {
            // Include types that inherit from T or implement the interface
            return (T)instances.Values.FirstOrDefault(x => x.GetType() == typeof(T) || x.GetType().IsSubclassOf(typeof(T)) || typeof(T).IsAssignableFrom(x.GetType()));
        }

        public static void InjectDependencies(object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                Dependency dep = field.GetCustomAttribute<Dependency>();
                if (dep != null)
                {
                    object value = Resolve(field.FieldType);
                    if (value == null && !dep.Optional)
                    {
                        throw new Exception($"Failed to resolve dependency of type {field.FieldType}");
                    }
                    field.SetValue(obj, value);
                }
            }
        }

        public static Type[] GetDependencies(Type type)
        {
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            return fields.Where(x => x.GetCustomAttribute<Dependency>() != null).Select(x => x.FieldType).ToArray();
        }
    }
}