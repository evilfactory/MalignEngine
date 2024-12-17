using System.Numerics;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine;

public struct SceneComponent : IComponent
{
    public AssetHandle<Scene> Scene;
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

            foreach (MemberInfo member in component.GetType().GetMembers())
            {
                DataFieldAttribute? dataField = member.GetCustomAttribute<DataFieldAttribute>();
                if (dataField == null || !dataField.Save) { continue; }

                Type memberType = member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
                object value = member is PropertyInfo ? ((PropertyInfo)member).GetValue(component) : ((FieldInfo)member).GetValue(component);

                if (memberType == typeof(int) || memberType == typeof(float) || memberType == typeof(string) || memberType == typeof(bool))
                {
                    componentElement.Add(new XAttribute(dataField.Name, value));
                }
                else if (memberType == typeof(Vector2))
                {
                    componentElement.Add(new XAttribute(dataField.Name, $"{((Vector2)value).X},{((Vector2)value).Y}"));
                }
                else if (memberType == typeof(Vector3))
                {
                    componentElement.Add(new XAttribute(dataField.Name, $"{((Vector3)value).X},{((Vector3)value).Y},{((Vector3)value).Z}"));
                }
                else if (memberType == typeof(Vector4))
                {
                    componentElement.Add(new XAttribute(dataField.Name, $"{((Vector4)value).X},{((Vector4)value).Y},{((Vector4)value).Z},{((Vector4)value).W}"));
                }
                else if (memberType == typeof(Color))
                {
                    componentElement.Add(new XAttribute(dataField.Name, $"{((Color)value).R},{((Color)value).G},{((Color)value).B},{((Color)value).A}"));
                }
                else if (memberType == typeof(EntityRef))
                {
                    componentElement.Add(new XAttribute(dataField.Name, ((EntityRef)value).Id));
                }
                else
                {
                    Logger.LogWarning($"Unknown type {memberType}");
                }
            }

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
                foreach (MemberInfo member in componentType.GetMembers())
                {
                    DataFieldAttribute? dataField = member.GetCustomAttribute<DataFieldAttribute>();
                    if (dataField == null) { continue; }

                    string name = componentElement.Attribute(dataField.Name)?.Value;
                    if (name == null)
                    {
                        continue;
                    }

                    string value = componentElement.Attribute(dataField.Name)?.Value;

                    Type memberType = member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
                    Action<object, object> setValue = member is PropertyInfo ? ((PropertyInfo)member).SetValue : ((FieldInfo)member).SetValue;

                    if (memberType == typeof(int))
                    {
                        setValue(component, int.Parse(value));
                    }
                    else if (memberType == typeof(float))
                    {
                        setValue(component, float.Parse(value));
                    }
                    else if (memberType == typeof(string))
                    {
                        setValue(component, value);
                    }
                    else if (memberType == typeof(bool))
                    {
                        setValue(component, bool.Parse(value));
                    }
                    else if (memberType == typeof(Vector2))
                    {
                        string[] parts = value.Split(',');
                        setValue(component, new Vector2(float.Parse(parts[0]), float.Parse(parts[1])));
                    }
                    else if (memberType == typeof(Vector3))
                    {
                        string[] parts = value.Split(',');
                        setValue(component, new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2])));
                    }
                    else if (memberType == typeof(Vector4))
                    {
                        string[] parts = value.Split(',');
                        setValue(component, new Vector4(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                    }
                    else if (memberType == typeof(Color))
                    {
                        string[] parts = value.Split(',');
                        setValue(component, new Color(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                    }
                    else if (memberType == typeof(EntityRef))
                    {
                        setValue(component, idRemap[int.Parse(value)]);
                    }
                    else
                    {
                        Logger.LogWarning($"Unknown type {memberType}");
                    }
                }

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

        return root;
    }
}