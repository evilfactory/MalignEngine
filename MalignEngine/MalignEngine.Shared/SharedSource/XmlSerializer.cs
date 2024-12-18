using System.Numerics;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine
{
    public interface ICustomXmlSerializer
    {
        bool SupportsType(Type type);
        void Serialize(object value, string dataFieldName, XElement element);
        object? Deserialize(string dataFieldName, XElement element, Dictionary<int, EntityRef> idRemap);
    }

    public static class XmlSerializer
    {
        private static List<ICustomXmlSerializer> serializers = new List<ICustomXmlSerializer>();

        static XmlSerializer()
        {
            // Get serializers from ALL assemblies
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
            {
                if (type.GetInterfaces().Contains(typeof(ICustomXmlSerializer)))
                {
                    serializers.Add((ICustomXmlSerializer)Activator.CreateInstance(type));
                }
            }
        }

        private static bool IsSupportedType(Type type)
        {
            return type == typeof(int) || type == typeof(float) || type == typeof(string) || type == typeof(bool) ||
                   type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) || type == typeof(Color) ||
                   type == typeof(EntityRef) || type == typeof(Quaternion);
        }

        public static void SerializeObject(object obj, XElement element)
        {
            foreach (MemberInfo member in element.GetType().GetMembers())
            {
                DataFieldAttribute? dataField = member.GetCustomAttribute<DataFieldAttribute>();
                if (dataField == null || !dataField.Save) { continue; }

                Type memberType = member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
                object value = member is PropertyInfo ? ((PropertyInfo)member).GetValue(element) : ((FieldInfo)member).GetValue(element);

                if (memberType == typeof(int) || memberType == typeof(float) || memberType == typeof(string) || memberType == typeof(bool))
                {
                    element.Add(new XAttribute(dataField.Name, value));
                }
                else if (memberType == typeof(Vector2))
                {
                    element.Add(new XAttribute(dataField.Name, $"{((Vector2)value).X},{((Vector2)value).Y}"));
                }
                else if (memberType == typeof(Vector3))
                {
                    element.Add(new XAttribute(dataField.Name, $"{((Vector3)value).X},{((Vector3)value).Y},{((Vector3)value).Z}"));
                }
                else if (memberType == typeof(Vector4))
                {
                    element.Add(new XAttribute(dataField.Name, $"{((Vector4)value).X},{((Vector4)value).Y},{((Vector4)value).Z},{((Vector4)value).W}"));
                }
                else if (memberType == typeof(Color))
                {
                    element.Add(new XAttribute(dataField.Name, $"{((Color)value).R},{((Color)value).G},{((Color)value).B},{((Color)value).A}"));
                }
                else if (memberType == typeof(EntityRef))
                {
                    element.Add(new XAttribute(dataField.Name, ((EntityRef)value).Id));
                }
                else
                {
                    bool handled = false;
                    foreach (ICustomXmlSerializer serializer in serializers)
                    {
                        if (serializer.SupportsType(memberType))
                        {
                            serializer.Serialize(value, dataField.Name, element);
                            handled = true;
                            break;
                        }
                    }

                    if (!handled)
                    {
                        throw new Exception($"Unknown type {memberType}");
                    }
                }
            }
        }

        public static void DeserializeObject(in object obj, XElement element, Dictionary<int, EntityRef> idRemap)
        {
            foreach (MemberInfo member in obj.GetType().GetMembers())
            {
                DataFieldAttribute? dataField = member.GetCustomAttribute<DataFieldAttribute>();
                if (dataField == null) { continue; }

                Type memberType = member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
                Action<object, object> setValue = member is PropertyInfo ? ((PropertyInfo)member).SetValue : ((FieldInfo)member).SetValue;


                if (!IsSupportedType(memberType))
                {
                    bool handled = false;
                    foreach (ICustomXmlSerializer serializer in serializers)
                    {
                        if (serializer.SupportsType(memberType))
                        {
                            setValue(obj, serializer.Deserialize(dataField.Name, element, idRemap));
                            handled = true;
                            break;
                        }
                    }

                    if (!handled)
                    {
                        throw new Exception($"Unknown type {memberType}");
                    }

                    continue;
                }

                string name = element.Attribute(dataField.Name)?.Value;
                if (name == null)
                {
                    continue;
                }

                string value = element.Attribute(dataField.Name)?.Value;

                if (memberType == typeof(int))
                {
                    setValue(obj, int.Parse(value));
                }
                else if (memberType == typeof(float))
                {
                    setValue(obj, float.Parse(value));
                }
                else if (memberType == typeof(string))
                {
                    setValue(obj, value);
                }
                else if (memberType == typeof(bool))
                {
                    setValue(obj, bool.Parse(value));
                }
                else if (memberType == typeof(Vector2))
                {
                    string[] parts = value.Split(',');
                    setValue(obj, new Vector2(float.Parse(parts[0]), float.Parse(parts[1])));
                }
                else if (memberType == typeof(Vector3))
                {
                    string[] parts = value.Split(',');
                    setValue(obj, new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2])));
                }
                else if (memberType == typeof(Vector4))
                {
                    string[] parts = value.Split(',');
                    setValue(obj, new Vector4(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                }
                else if (memberType == typeof(Color))
                {
                    string[] parts = value.Split(',');
                    setValue(obj, new Color(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                }
                else if (memberType == typeof(EntityRef))
                {
                    setValue(obj, idRemap[int.Parse(value)]);
                }
            }
        }
    }
}