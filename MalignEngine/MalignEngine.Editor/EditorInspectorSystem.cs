using ImGuiNET;
using nkast.Aether.Physics2D.Dynamics;
using System.Collections;
using System.Numerics;
using System.Reflection;

namespace MalignEngine.Editor;

public class EditorInspectorSystem : BaseEditorWindowSystem
{
    private IEntityManager _entityManager;
    private ParentSystem _parentSystem;

    public override string WindowName => "Inspector";

    public EditorInspectorSystem(EditorSystem editorSystem, ImGuiService imGuiService, IEntityManager entityManager, ParentSystem parentSystem) : base(editorSystem, imGuiService)
    {
        _entityManager = entityManager;
        _parentSystem = parentSystem;
    }

    private void RecursiveEntityTree(EntityRef[] entities)
    {
        foreach (EntityRef entity in entities)
        {
            string name = "Unknown";
            if (entity.TryGet(out NameComponent nameComponent))
            {
                name = nameComponent.Name;
            }

            if (entity.TryGet(out Children children))
            {
                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;

                if (EditorSystem.SelectedEntity == entity)
                {
                    flags |= ImGuiTreeNodeFlags.Selected;
                }

                if (ImGui.TreeNodeEx($"{name} - {entity.Id}", flags))
                {
                    if (ImGui.IsItemClicked())
                    {
                        EditorSystem.SelectedEntity = entity;
                    }

                    RecursiveEntityTree(children.Childs.ToArray());
                    ImGui.TreePop();
                }
                else
                {
                    if (ImGui.IsItemClicked())
                    {
                        EditorSystem.SelectedEntity = entity;
                    }
                }
            }
            else
            {
                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.Leaf;

                if (EditorSystem.SelectedEntity == entity)
                {
                    flags |= ImGuiTreeNodeFlags.Selected;
                }

                if (ImGui.TreeNodeEx($"{name} - {entity.Id}", flags))
                {
                    if (ImGui.IsItemClicked())
                    {
                        EditorSystem.SelectedEntity = entity;
                    }

                    ImGui.TreePop();
                }
                else
                {
                    if (ImGui.IsItemClicked())
                    {
                        EditorSystem.SelectedEntity = entity;
                    }
                }
            }
        }
    }

    public override void DrawWindow(float deltaTime)
    {
        if (!ImGui.Begin("EntityDebugger", ImGuiWindowFlags.NoScrollbar)) { return; }

        if (ImGui.BeginTable("split", 2, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.Resizable))
        {

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            ImGui.BeginChild("scrolling", new Vector2(0, 0), false);

            if (ImGui.TreeNodeEx("Entities", ImGuiTreeNodeFlags.Selected))
            {
                RecursiveEntityTree(_parentSystem.RootEntities.ToArray());
            }

            ImGui.EndChild();

            ImGui.TableSetColumnIndex(1);

            ImGui.BeginChild("scrolling2", new Vector2(0, 0), false);

            if (_entityManager.World.IsValid(EditorSystem.SelectedEntity))
            {
                EntityRef entity = EditorSystem.SelectedEntity;

                if (ImGui.Button("Delete"))
                {
                    _entityManager.World.Destroy(entity);
                    ImGui.EndChild();
                    ImGui.EndTable();
                    ImGui.End();

                    return;
                }

                ImGui.Text($"Entity Id: {entity.Id}");
                ImGui.Text($"Entity Version: {entity.Version}");

                object[] components = entity.GetComponents();

                int i = 0;
                foreach (object component in components)
                {
                    ImGui.PushID(i);

                    ImGui.Separator();

                    Type type = component.GetType();

                    ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.2f, 1.0f), $"{type.Name}");

                    FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

                    foreach (FieldInfo field in fields)
                    {
                        DrawMember(entity, field, component);
                    }

                    PropertyInfo[] properties = component.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                    foreach (PropertyInfo property in properties)
                    {
                        DrawMember(entity, property, component);
                    }

                    ImGui.PopID();

                    i++;
                }
            }

            ImGui.EndChild();
            ImGui.EndTable();
        }

        ImGui.End();
    }

    private static string FormatObject(object obj)
    {
        if (obj == null) { return "Null"; }

        if (obj is EntityRef entity)
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

    private static void DrawMember(EntityRef entity, MemberInfo member, object obj)
    {
        Type type = member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
        Func<object, object> getValue = member is PropertyInfo ? ((PropertyInfo)member).GetValue : ((FieldInfo)member).GetValue;
        Action<object, object> setValue = member is PropertyInfo ? ((PropertyInfo)member).SetValue : ((FieldInfo)member).SetValue;

        if (type == typeof(float))
        {
            float v = (float)getValue(obj);
            if (ImGui.InputFloat(member.Name, ref v))
            {
                setValue(obj, v);
                entity.Set(obj);
            }
        }
        else if (type == typeof(int))
        {
            int v = (int)getValue(obj);
            if (ImGui.InputInt(member.Name, ref v))
            {
                setValue(obj, v);
                entity.Set(obj);
            }
        }
        else if (type == typeof(string))
        {
            string v = (string)getValue(obj) ?? "NULL";
            if (ImGui.InputText(member.Name, ref v, 100))
            {
                setValue(obj, v);
                entity.Set(obj);
            }
        }
        else if (type == typeof(bool))
        {
            bool v = (bool)getValue(obj);
            if (ImGui.Checkbox(member.Name, ref v))
            {
                setValue(obj, v);
                entity.Set(obj);
            }
        }
        else if (type == typeof(Vector2))
        {
            Vector2 v = (Vector2)getValue(obj);
            if (ImGui.InputFloat2(member.Name, ref v))
            {
                setValue(obj, v);
                entity.Set(obj);
            }
        }
        else if (type == typeof(Vector3))
        {
            Vector3 v = (Vector3)getValue(obj);
            if (ImGui.InputFloat3(member.Name, ref v))
            {
                setValue(obj, v);
                entity.Set(obj);
            }
        }
        else if (type == typeof(Vector4))
        {
            Vector4 v = (Vector4)getValue(obj);
            if (ImGui.InputFloat4(member.Name, ref v))
            {
                setValue(obj, v);
                entity.Set(obj);
            }
        }
        else if (type == typeof(Color))
        {
            Color v = (Color)getValue(obj);
            Vector4 col = new Vector4(v.R / 255f, v.G / 255f, v.B / 255f, v.A / 255f);
            if (ImGui.ColorPicker4(member.Name, ref col))
            {
                v = new Color(col.X, col.Y, col.Z, col.W);
                setValue(obj, v);
                entity.Set(obj);
            }
        }
        else if (type.IsAssignableTo(typeof(IDictionary)))
        {
            ImGui.Text(member.Name);
            ImGui.Indent();
            IDictionary dict = (IDictionary)getValue(obj);
            foreach (var key in dict.Keys)
            {
                ImGui.Text($"{FormatObject(key)}: {FormatObject(dict[key])}");
            }
            ImGui.Unindent();
        }
        else
        {
            ImGui.Text($"{member.Name}: {FormatObject(getValue(obj))}");
        }
    }
}