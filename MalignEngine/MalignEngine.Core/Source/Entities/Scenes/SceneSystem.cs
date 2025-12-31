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
    private readonly ParentSystem _parentSystem;

    public SceneSystem(ILoggerService loggerService, IScheduleManager scheduleManager, IEntityManager entityManager, IEventService eventService, ParentSystem parentSystem) 
        : base(loggerService, scheduleManager, entityManager, eventService)
    {
        _parentSystem = parentSystem;
    }

    public EntityRef Instantiate(Scene scene)
    {
        List<EntityRef> otherEntities = new List<EntityRef>();

        scene.SceneWorld.Query(scene.SceneWorld.CreateQuery(), (EntityRef entity) =>
        {
            if (entity.Id == scene.Root.Id)
            {
                return;
            }

            otherEntities.Add(entity);
        });

        EntityIdRemap remap = new EntityIdRemap();

        EntityRef newRoot = EntityManager.World.CreateEntity();
        remap.AddEntity(scene.Root.Id, newRoot);

        List<EntityRef> otherNewEntities = new List<EntityRef>();
        otherEntities.ForEach((EntityRef entity) =>
        {
            EntityRef newEntity = EntityManager.World.CreateEntity();
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
            newRoot.Add(new SceneComponent() { SceneId = scene.SceneId });
        }

        return newRoot;
    }

    public static void CopyComponent(IComponent component, EntityRef from, EntityRef to, EntityIdRemap remap)
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

            if (value is EntityRef entityRef)
            {
                EntityRef newEntityRef = remap.GetEntity(entityRef.Id);
                setValue(newComponent, newEntityRef);
            }
            else
            {
                setValue(newComponent, value);
            }
        }

        to.Add(newComponent);
    }

    public static void CopyEntity(EntityRef from, EntityRef to, EntityIdRemap remap)
    {
        foreach (var component in from.GetComponents())
        {
            if (component is EntityMetaData)
            {
                continue;
            }

            CopyComponent(component, from, to, remap);
        }
    }
}
