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
            return instances[type];
        }

        public static T Resolve<T>()
        {
            return (T)instances[typeof(T)];
        }

        public static void InjectDependencies(object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<Dependency>() != null)
                {
                    field.SetValue(obj, Resolve(field.FieldType));
                }
            }
        }
    }
}