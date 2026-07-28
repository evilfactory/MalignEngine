using ImGuiNET;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine.Editor;

public interface IMemberEditor
{
    bool SupportsType(Type type);
    bool Draw(MemberInfo member, DataFieldAttribute? dataField, ref object value, bool readOnly);
}

public class PropertyEditor : IService
{
    private List<IMemberEditor> _memberEditors;

    public PropertyEditor(IEnumerable<IMemberEditor> memberEditors)
    {
        _memberEditors = memberEditors.ToList();
    }

    public bool DrawMembers(ref object serializableObject, bool showNonSerializable, bool allowEditingNonSerializable)
    {
        Type type = serializableObject.GetType();

        MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public);

        bool anyChanged = false;

        foreach (MemberInfo member in members)
        {
            DataFieldAttribute? dataField = member.GetCustomAttribute<DataFieldAttribute>();

            if (!showNonSerializable && dataField == null) { continue; }

            anyChanged = DrawMember(ref serializableObject, member, !allowEditingNonSerializable && dataField == null) || anyChanged;
        }

        return anyChanged;
    }

    private bool DrawMember(ref object serializableObject, MemberInfo member, bool readOnly)
    {
        if (member is not PropertyInfo && member is not FieldInfo) { return false; }
        DataFieldAttribute? dataField = member.GetCustomAttribute<DataFieldAttribute>();

        Type type = member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
        Func<object, object> getValue = member is PropertyInfo ? ((PropertyInfo)member).GetValue : ((FieldInfo)member).GetValue;
        Action<object, object> setValue = member is PropertyInfo ? ((PropertyInfo)member).SetValue : ((FieldInfo)member).SetValue;

        IMemberEditor? memberEditor = _memberEditors.FirstOrDefault(x => x.SupportsType(type));

        object value = getValue(serializableObject);

        if (memberEditor == null)
        {
            ImGui.Text($"{member.Name}: {FormatObject(value)}");
            return false;
        }

        if (memberEditor.Draw(member, dataField, ref value, readOnly))
        {
            setValue(serializableObject, value);
            return true;
        }

        return false;
    }

    public static string FormatObject(object obj)
    {
        if (obj == null) { return "Null"; }

        if (obj is Entity entity)
        {
            if (entity.Has<NameComponent>())
            {
                return entity.Get<NameComponent>().Name;
            }
            else
            {
                return $"Unnamed - {entity.Id}";
            }
        }

        if (obj is Type type)
        {
            return type.Name;
        }

        return obj.ToString();
    }
}