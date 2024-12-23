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
    [Dependency]
    protected ParentSystem ParentSystem = default!;

    protected ILogger Logger;

    public override void OnInitialize()
    {
        Logger = LoggerService.GetSawmill("scenes");
    }

    private void SaveEntity(EntityRef entity, XElement element)
    {
        element.Add(new XAttribute("Id", entity.Id));

        foreach (IComponent component in entity.GetComponents())
        {
            // Check if serializable
            if (component.GetType().GetCustomAttribute<SerializableAttribute>() == null)
            {
                continue;
            }

            XElement componentElement = new XElement(component.GetType().Name);

            XmlSerializer.SerializeObject(component, componentElement);

            element.Add(componentElement);
        }
    }

    public Scene SaveScene(EntityRef root)
    {
        Logger.LogVerbose("Saving scene");

        XElement sceneXml = new XElement("Scene");

        void saveToElement(EntityRef entity)
        {
            XElement entityXml = new XElement("Entity");
            sceneXml.Add(entityXml);
            SaveEntity(entity, entityXml);

            if (entity.TryGet(out Children children))
            {
                foreach (EntityRef child in children.Childs)
                {
                    saveToElement(child);
                }
            }
        }

        saveToElement(root);

        return new Scene(sceneXml);
    }

    public EntityRef LoadScene(Scene scene)
    {
        Logger.LogVerbose($"Loading scene: {scene}");

        if (scene.customLoadAction != null)
        {
            return scene.customLoadAction(EntityManager.World);
        }

        // Search for all types that implement IComponent, very slow
        Dictionary<string, Type> components = new Dictionary<string, Type>();
        foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(ass => ass.GetTypes()))
        {
            if (type.GetInterfaces().Contains(typeof(IComponent)))
            {
                components.Add(type.Name, type);
            }
        }

        XElement xml = scene.SceneData;

        Dictionary<int, EntityRef> idRemap = new Dictionary<int, EntityRef>();

        List<(EntityRef, XElement)> componentsToLoad = new List<(EntityRef, XElement)>();

        foreach (XElement element in xml.Elements())
        {
            EntityRef entityRef = EntityManager.World.CreateEntity();

            // Get id
            int id = int.Parse(element.Attribute("Id").Value);
            idRemap.Add(id, entityRef);

            componentsToLoad.Add((entityRef, element));
        }

        // Load components
        foreach ((EntityRef entity, XElement element) in componentsToLoad)
        {
            foreach (XElement componentElement in element.Elements()) 
            {
                if (!components.ContainsKey(componentElement.Name.LocalName))
                {
                    Logger.LogWarning($"Unknown component {componentElement.Name.LocalName}");
                    continue;
                }

                Type componentType = components[componentElement.Name.LocalName];

                // Check if serializable
                if (componentType.GetCustomAttribute<SerializableAttribute>() == null)
                {
                    Logger.LogWarning($"Non-serializable component attempted to load {componentElement.Name.LocalName}");
                    continue;
                }

                // Create component instance
                IComponent component = Activator.CreateInstance(componentType) as IComponent;

                if (component == null)
                {
                    throw new Exception($"Failed to create component {componentType}");
                }
                
                // Go through all fields and properties that have DataField attribute
                XmlSerializer.DeserializeObject(component, componentElement, idRemap);

                entity.Add(component);
            }
        }

        EntityRef root = EntityRef.Null;

        foreach ((EntityRef entity, XElement element) in componentsToLoad)
        {
            if (!entity.Has<ParentOf>())
            {
                if (root != EntityRef.Null)
                {
                    Logger.LogWarning("Multiple root entities found");
                }

                root = entity;
            }
        }

        root.Add(new SceneComponent() { SceneId = scene.SceneId });

        return root;
    }
}