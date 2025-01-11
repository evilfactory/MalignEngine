using System.Numerics;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine
{
    public interface ICustomTypeXmlSerializer
    {
        bool SupportsType(Type type);
        void Serialize(object value, string dataFieldName, XElement element);
        object? Deserialize(string dataFieldName, XElement element, EntityIdRemap? idRemap = null);
    }

    public interface ICustomObjectXmlSerializer
    {
        bool SupportsType(object obj);
        void Serialize(object obj, XElement element);
        void Deserialize(object obj, XElement element, EntityIdRemap? idRemap = null);
    }

    public static class XmlSerializer
    {
        private static List<ICustomTypeXmlSerializer> serializers = new List<ICustomTypeXmlSerializer>();
        private static List<ICustomObjectXmlSerializer> objectSerializers = new List<ICustomObjectXmlSerializer>();


        static XmlSerializer()
        {
            // Get serializers from ALL assemblies
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
            {
                if (type.GetInterfaces().Contains(typeof(ICustomTypeXmlSerializer)))
                {
                    serializers.Add((ICustomTypeXmlSerializer)Activator.CreateInstance(type));
                }

                if (type.GetInterfaces().Contains(typeof(ICustomObjectXmlSerializer)))
                {
                    objectSerializers.Add((ICustomObjectXmlSerializer)Activator.CreateInstance(type));
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
            // check if any serializer supports serializing this object as a whole
            foreach (ICustomObjectXmlSerializer serializer in objectSerializers)
            {
                if (serializer.SupportsType(obj))
                {
                    serializer.Serialize(obj, element);
                    return;
                }
            }

            foreach (MemberInfo member in obj.GetType().GetMembers())
            {
                DataFieldAttribute? dataField = member.GetCustomAttribute<DataFieldAttribute>();
                if (dataField == null || !dataField.Save) { continue; }

                Type memberType = member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
                object value = member is PropertyInfo ? ((PropertyInfo)member).GetValue(obj) : ((FieldInfo)member).GetValue(obj);

                if (memberType == typeof(int) || memberType == typeof(float) || memberType == typeof(string) || memberType == typeof(bool))
                {
                    element.SetAttributeValue(dataField.Name, value);
                }
                else if (memberType == typeof(Vector2))
                {
                    element.SetAttributeVector2(dataField.Name, (Vector2)value);
                }
                else if (memberType == typeof(Vector3))
                {
                    element.SetAttributeVector3(dataField.Name, (Vector3)value);
                }
                else if (memberType == typeof(Vector4))
                {
                    element.SetAttributeVector4(dataField.Name, (Vector4)value);
                }
                else if (memberType == typeof(Color))
                {
                    element.SetAttributeColor(dataField.Name, (Color)value);
                }
                else if (memberType == typeof(EntityRef))
                {
                    element.SetAttributeInt(dataField.Name, ((EntityRef)value).Id);
                }
                else
                {
                    bool handled = false;
                    foreach (ICustomTypeXmlSerializer serializer in serializers)
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

        public static void DeserializeObject(in object obj, XElement element, EntityIdRemap? idRemap = null)
        {
            // check if any serializer supports deserializing this object as a whole
            foreach (ICustomObjectXmlSerializer serializer in objectSerializers)
            {
                if (serializer.SupportsType(obj))
                {
                    serializer.Deserialize(obj, element, idRemap);
                    return;
                }
            }

            foreach (MemberInfo member in obj.GetType().GetMembers())
            {
                DataFieldAttribute? dataField = member.GetCustomAttribute<DataFieldAttribute>();
                if (dataField == null) { continue; }

                Type memberType = member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
                Action<object, object> setValue = member is PropertyInfo ? ((PropertyInfo)member).SetValue : ((FieldInfo)member).SetValue;


                if (!IsSupportedType(memberType))
                {
                    bool handled = false;
                    foreach (ICustomTypeXmlSerializer serializer in serializers)
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

                XAttribute? attribute = element.Attribute(dataField.Name);

                if (attribute == null) { continue; }

                if (memberType == typeof(int))
                {
                    setValue(obj, element.GetAttributeInt(dataField.Name));
                }
                else if (memberType == typeof(float))
                {
                    setValue(obj, element.GetAttributeFloat(dataField.Name));
                }
                else if (memberType == typeof(string))
                {
                    setValue(obj, element.GetAttributeString(dataField.Name));
                }
                else if (memberType == typeof(bool))
                {
                    setValue(obj, element.GetAttributeBool(dataField.Name));
                }
                else if (memberType == typeof(Vector2))
                {
                    setValue(obj, element.GetAttributeVector2(dataField.Name));
                }
                else if (memberType == typeof(Vector3))
                {
                    setValue(obj, element.GetAttributeVector3(dataField.Name));
                }
                else if (memberType == typeof(Vector4))
                {
                    setValue(obj, element.GetAttributeVector4(dataField.Name));
                }
                else if (memberType == typeof(Color))
                {
                    setValue(obj, element.GetAttributeColor(dataField.Name));
                }
                else if (memberType == typeof(EntityRef))
                {
                    if (idRemap == null)
                    {
                        throw new Exception("EntityRef encountered but no idRemap provided");
                    }
                    setValue(obj, idRemap.GetEntity(element.GetAttributeInt(dataField.Name)));
                }
            }
        }
    }
}