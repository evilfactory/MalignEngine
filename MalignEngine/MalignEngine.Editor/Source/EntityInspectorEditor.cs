using ImGuiNET;
using nkast.Aether.Physics2D.Dynamics;
using System.Collections;
using System.Numerics;
using System.Reflection;

namespace MalignEngine.Editor;

public class EntityInspectorEditor : BaseEditorWindowSystem
{
    private readonly IEntityManager _entityManager;
    private readonly HierarchySystem _parentSystem;
    private readonly PropertyEditor _propertyEditor;

    private bool _showNonSerializable = true;
    private bool _allowEditingNonSerializable;

    public override string WindowName => "Inspector";

    public EntityInspectorEditor(IServiceContainer serviceContainer, EditorSystem editorSystem, ImGuiSystem imGuiService, PropertyEditor propertyEditor, IEntityManager entityManager, HierarchySystem parentSystem)
        : base(serviceContainer, editorSystem, imGuiService)
    {
        _entityManager = entityManager;
        _parentSystem = parentSystem;
        _propertyEditor = propertyEditor;
    }

    private void RecursiveEntityTree(Entity[] entities)
    {
        foreach (Entity entity in entities)
        {
            string name = "Unknown";
            if (entity.TryGet(out ComponentRef<NameComponent> nameComponent))
            {
                name = nameComponent.Value.Name;
            }

            Vector4? color = null;

            if (entity.TryGet(out ComponentRef<SceneComponent> sceneComponent))
            {
                color = new Vector4(0f, 0.2f, 0.9f, 1f);
            }

            if (entity.TryGet(out ComponentRef<Children> children))
            {
                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;

                if (EditorSystem.SelectedEntity == entity)
                {
                    flags |= ImGuiTreeNodeFlags.Selected;
                }

                if (color != null) { ImGui.PushStyleColor(ImGuiCol.Text, color.Value); }
                if (ImGui.TreeNodeEx($"{name} - {entity.Id}", flags))
                {
                    if (color != null) { ImGui.PopStyleColor(); }

                    if (ImGui.IsItemClicked())
                    {
                        EditorSystem.SelectedEntity = entity;
                    }

                    RecursiveEntityTree(children.Value.Values.ToArray());
                    ImGui.TreePop();
                }
            }
            else
            {
                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.Leaf;

                if (EditorSystem.SelectedEntity == entity)
                {
                    flags |= ImGuiTreeNodeFlags.Selected;
                }

                if (color != null) { ImGui.PushStyleColor(ImGuiCol.Text, color.Value); }
                if (ImGui.TreeNodeEx($"{name} - {entity.Id}", flags))
                {
                    if (color != null) { ImGui.PopStyleColor(); }

                    if (ImGui.IsItemClicked())
                    {
                        EditorSystem.SelectedEntity = entity;
                    }

                    ImGui.TreePop();
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

            if (_entityManager.World.IsAlive(EditorSystem.SelectedEntity))
            {
                Entity entity = EditorSystem.SelectedEntity;

                ImGui.Checkbox("Show non-serializable", ref _showNonSerializable);
                ImGui.Checkbox("Enable editing non-serializable", ref _allowEditingNonSerializable);

                if (ImGui.Button("Delete"))
                {
                    _entityManager.Destroy(entity);
                    ImGui.EndChild();
                    ImGui.EndTable();
                    ImGui.End();

                    return;
                }

                ImGui.Text($"Entity Id: {entity.Id}");
                ImGui.Text($"Entity Version: {entity.Version}");

                IComponent[] components = _entityManager.World.GetComponents(entity).ToArray();

                int i = 0;
                foreach (IComponent component in components)
                {
                    ImGui.PushID(i);

                    ImGui.Separator();

                    Type type = component.GetType();

                    ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.2f, 1.0f), $"{type.Name}");

                    object refComp = component;
                    if (_propertyEditor.DrawMembers(ref refComp, _showNonSerializable, _allowEditingNonSerializable))
                    {
                        entity.AddOrSet((IComponent)refComp);
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
}