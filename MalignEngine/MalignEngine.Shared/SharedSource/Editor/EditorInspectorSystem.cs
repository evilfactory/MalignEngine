using Arch.Core;
using Arch.Core.Extensions;
using ImGuiNET;
using nkast.Aether.Physics2D.Dynamics;
using System.Numerics;
using System.Reflection;

namespace MalignEngine
{
    public class EditorInspectorSystem : BaseEditorWindowSystem
    {
        public override string WindowName => "Inspector";

        public override void Draw(float deltaTime)
        {
            if (!Active) { return; }

            if (!ImGui.Begin("EntityDebugger", ImGuiWindowFlags.NoScrollbar)) { return; }

            if (ImGui.BeginTable("split", 2, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.Resizable))
            {

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                ImGui.BeginChild("scrolling", new Vector2(0, 0), false);

                var query = new QueryDescription();
                World.Query(in query, (Entity entity) =>
                {
                    string name = "Unnamed";

                    if (ImGui.Selectable($"{name} - {entity.Id}", EditorSystem.SelectedEntity.Entity == entity))
                    {
                        EditorSystem.SelectedEntity = entity.Reference();
                    }
                });

                ImGui.EndChild();

                ImGui.TableSetColumnIndex(1);

                ImGui.BeginChild("scrolling2", new Vector2(0, 0), false);

                if (EditorSystem.SelectedEntity != EntityReference.Null)
                {
                    Entity entity = EditorSystem.SelectedEntity.Entity;

                    ImGui.Text($"Entity Id: {entity.Id}");
                    ImGui.Text($"Entity Version: {entity.Version()}");

                    object[] components = entity.GetAllComponents();

                    foreach (object component in components)
                    {
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

            if (obj is Entity entity)
            {
                //if (entity.Has<NameComponent>())
                //{
                //    return entity.Get<NameComponent>().Name;
                //}
                //else
                //{
                    return $"Unnamed - {entity.Id}";
                //}
            }

            return obj.ToString();
        }

        private static void DrawMember(Entity entity, MemberInfo member, object obj)
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
            else
            {
                ImGui.Text($"{member.Name}: {FormatObject(getValue(obj))}");
            }
        }
    }
}