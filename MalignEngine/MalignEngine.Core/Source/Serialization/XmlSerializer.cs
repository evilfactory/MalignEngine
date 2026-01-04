using System.Numerics;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine;

public class XmlSerializer : IService
{
    private IAssetService _assetService;

    public XmlSerializer(IAssetService assetService)
    {
        _assetService = assetService;
    }

    private bool IsSupportedType(Type type)
    {
        return type == typeof(int) || type == typeof(float) || type == typeof(string) || type == typeof(bool) ||
               type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) || type == typeof(Color) ||
               type == typeof(Entity) || type == typeof(Quaternion) || type.IsAssignableTo(typeof(IAssetHandle));
    }

    public void SerializeObject(object obj, XElement element, bool saveAll = false)
    {
        foreach (MemberInfo member in obj.GetType().GetMembers())
        {
            DataFieldAttribute? dataField = member.GetCustomAttribute<DataFieldAttribute>();
            if (dataField == null || (!dataField.Save && !saveAll)) { continue; }

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
            else if (memberType == typeof(Entity))
            {
                element.SetAttributeInt(dataField.Name, ((Entity)value).Id);
            }
            else if (memberType.IsEnum)
            {
                element.SetAttributeString(dataField.Name, Enum.GetName(memberType, value));
            }
            else if (value is IAssetHandle handle)
            {
                element.SetAttributeString(dataField.Name, handle.AssetPath);
            }
            else
            {
                bool handled = false;


                if (!handled)
                {
                    throw new Exception($"Unknown type {memberType}");
                }
            }
        }
    }

    public void DeserializeObject(in object obj, XElement element, EntityIdRemap? idRemap = null)
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
            else if (memberType.IsEnum)
            {
                setValue(obj, Enum.Parse(memberType, element.GetAttributeString(dataField.Name)));
            }
            else if (memberType == typeof(Entity))
            {
                if (idRemap == null)
                {
                    throw new Exception("EntityRef encountered but no idRemap provided");
                }
                setValue(obj, idRemap.GetEntity(element.GetAttributeInt(dataField.Name)));
            }
            else if (memberType.IsAssignableTo(typeof(IAssetHandle)))
            {
                AssetHandle handle = _assetService.FromPath(element.GetAttributeString(dataField.Name));

                setValue(obj, handle.Upgrade(memberType.GetGenericArguments()[0]));
            }
        }
    }
}