using System.Reflection;

namespace MalignEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    public class Dependency : Attribute { }

    public static class IoCManager
    {
        private static readonly Dictionary<Type, object> instances = new Dictionary<Type, object>();


        public static void Register(object instance)
        {
            instances[instance.GetType()] = instance;
        }

        public static object Resolve(Type type)
        {
            // Include types that inherit from type
            return instances.Values.FirstOrDefault(x => x.GetType() == type || x.GetType().IsSubclassOf(type));
        }

        public static T Resolve<T>()
        {
            // Include types that inherit from T
            return (T)instances.Values.FirstOrDefault(x => x.GetType() == typeof(T) || x.GetType().IsSubclassOf(typeof(T)));
        }

        public static void InjectDependencies(object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<Dependency>() != null)
                {
                    object value = Resolve(field.FieldType);
                    if (value == null)
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