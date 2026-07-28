
using ImGuiNET;
using System.Collections;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MalignEngine.Editor;

public class DefaultMemberEditors : IMemberEditor
{
    public bool SupportsType(Type type)
    {
        return type == typeof(int) ||
            type == typeof(float) ||
            type == typeof(string) ||
            type == typeof(bool) ||
            type == typeof(Vector2) ||
            type == typeof(Vector3) ||
            type == typeof(Vector4) ||
            type == typeof(Color) ||
            type.IsAssignableTo(typeof(IDictionary));
    }

    public bool Draw(MemberInfo member, DataFieldAttribute? dataField, ref object value, bool readOnly)
    {
        Type type = value.GetType();

        if (readOnly)
        {
            ImGui.BeginDisabled();
        }

        bool edited = false;

        if (type == typeof(float))
        {
            float v = (float)value;
            if (ImGui.InputFloat(member.Name, ref v))
            {
                value = v;
                edited = true;
            }
        }
        else if (type == typeof(int))
        {
            int v = (int)value;
            if (ImGui.InputInt(member.Name, ref v))
            {
                value = v;
                edited = true;
            }
        }
        else if (type == typeof(string))
        {
            string v = (string)value ?? "NULL";
            if (ImGui.InputText(member.Name, ref v, 100))
            {
                value = v;
                edited = true;
            }
        }
        else if (type == typeof(bool))
        {
            bool v = (bool)value;
            if (ImGui.Checkbox(member.Name, ref v))
            {
                value = v;
                edited = true;
            }
        }
        else if (type == typeof(Vector2))
        {
            Vector2 v = (Vector2)value;
            if (ImGui.InputFloat2(member.Name, ref v))
            {
                value = v;
                edited = true;
            }
        }
        else if (type == typeof(Vector3))
        {
            Vector3 v = (Vector3)value;
            if (ImGui.InputFloat3(member.Name, ref v))
            {
                value = v;
                edited = true;
            }
        }
        else if (type == typeof(Vector4))
        {
            Vector4 v = (Vector4)value;
            if (ImGui.InputFloat4(member.Name, ref v))
            {
                value = v;
                edited = true;
            }
        }
        else if (type == typeof(Color))
        {
            Color v = (Color)value;
            Vector4 col = new Vector4(v.R / 255f, v.G / 255f, v.B / 255f, v.A / 255f);
            if (ImGui.ColorPicker4(member.Name, ref col))
            {
                v = new Color(col.X, col.Y, col.Z, col.W);
                value = v;
                edited = true;
            }
        }
        else if (type.IsAssignableTo(typeof(IDictionary)))
        {
            ImGui.Text(member.Name);
            ImGui.Indent();
            IDictionary dict = (IDictionary)value;
            foreach (var key in dict.Keys)
            {
                ImGui.Text($"{PropertyEditor.FormatObject(key)}: {PropertyEditor.FormatObject(dict[key])}");
            }
            ImGui.Unindent();
        }

        if (readOnly)
        {
            ImGui.EndDisabled();
        }

        return edited;
    }
}