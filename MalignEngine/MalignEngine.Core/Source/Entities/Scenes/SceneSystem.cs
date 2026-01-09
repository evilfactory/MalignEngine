using System.Numerics;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine;

public struct SceneComponent : IComponent
{
    public string SceneId;
}

public class SceneSystem : EntitySystem
{
    private readonly HierarchySystem _hierarchySystem;

    public SceneSystem(IServiceContainer serviceContainer, HierarchySystem parentSystem) 
        : base(serviceContainer)
    {
        _hierarchySystem = parentSystem;
    }

    public Entity Instantiate(Scene scene)
    {
        List<Entity> otherEntities = new List<Entity>();

        scene.SceneWorld.Query(new Query(), (Entity entity) =>
        {
            if (entity.Id == scene.Root.Id)
            {
                return;
            }

            otherEntities.Add(entity);
        });

        EntityIdRemap remap = new EntityIdRemap();

        Entity newRoot = EntityManager.World.CreateEntity();
        remap.AddEntity(scene.Root.Id, newRoot);

        List<Entity> otherNewEntities = new List<Entity>();
        otherEntities.ForEach((Entity entity) =>
        {
            Entity newEntity = EntityManager.World.CreateEntity();
            remap.AddEntity(entity.Id, newEntity);
            otherNewEntities.Add(newEntity);
        });

        CopyEntity(scene.Root, newRoot, remap);

        for (int i = 0; i < otherEntities.Count; i++)
        {
            CopyEntity(otherEntities[i], otherNewEntities[i], remap);
        }

        if (scene.SceneId != null)
        {
            newRoot.AddOrSet(new SceneComponent() { SceneId = scene.SceneId });
        }

        return newRoot;
    }

    public static void CopyComponent(IComponent component, Entity from, Entity to, EntityIdRemap remap)
    {
        Type componentType = component.GetType();

        // Create new instance of the component
        IComponent newComponent = (IComponent)Activator.CreateInstance(componentType);

        // get all datafield members of the component
        foreach (MemberInfo member in component.GetType().GetMembers())
        {
            if (member is not PropertyInfo && member is not FieldInfo)
            {
                continue;
            }

            object value = member is PropertyInfo ? ((PropertyInfo)member).GetValue(component) : ((FieldInfo)member).GetValue(component);
            Action<object, object> setValue = member is PropertyInfo ? ((PropertyInfo)member).SetValue : ((FieldInfo)member).SetValue;

            if (value is Entity entityRef)
            {
                Entity newEntityRef = remap.GetEntity(entityRef.Id);
                setValue(newComponent, newEntityRef);
            }
            else
            {
                setValue(newComponent, value);
            }
        }

        to.AddOrSet(newComponent);
    }

    public static void CopyEntity(Entity from, Entity to, EntityIdRemap remap)
    {
        foreach (var component in from.GetComponents())
        {
            CopyComponent(component, from, to, remap);
        }
    }
}
