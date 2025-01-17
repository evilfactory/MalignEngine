using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace MalignEngine;

public class EntityIdRemap
{
    public Dictionary<int, EntityRef> IdRemap
    {
        get; private set;
    }

    public EntityIdRemap()
    {
        IdRemap = new Dictionary<int, EntityRef>();
    }

    public void AddEntity(int id, EntityRef entity)
    {
        IdRemap.Add(id, entity);
    }

    public EntityRef GetEntity(int id)
    {
        return IdRemap[id];
    }

    public void Clear()
    {
        IdRemap.Clear();
    }

}

public class EntitySerializer
{
    private static Dictionary<string, Type> components;

    static EntitySerializer()
    {
        // Search for all types that implement IComponent, very slow
        components = new Dictionary<string, Type>();
        foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(ass => ass.GetTypes()))
        {
            if (type.GetInterfaces().Contains(typeof(IComponent)))
            {
                components.Add(type.Name, type);
            }
        }
    }

    public static void SerializeEntity(EntityRef entity, XElement data)
    {
        data.SetAttributeValue("Id", entity.Id.ToString());

        foreach (object component in entity.GetComponents())
        {
            XElement componentElement = new XElement(component.GetType().Name);

            XmlSerializer.SerializeObject(component, componentElement);

            data.Add(componentElement);
        }
    }

    public static void DeserializeEntity(EntityRef entity, XElement data, EntityIdRemap idRemap)
    {
        foreach (XElement componentElement in data.Elements())
        {
            if (!components.ContainsKey(componentElement.Name.LocalName))
            {
                throw new Exception($"Unknown component {componentElement.Name.LocalName}");
            }

            Type componentType = components[componentElement.Name.LocalName];

            if (componentType.GetCustomAttribute<SerializableAttribute>() == null)
            {
                throw new Exception($"Non-serializable component attempted to load {componentElement.Name.LocalName}");
            }

            IComponent? component = Activator.CreateInstance(componentType) as IComponent;

            if (component == null)
            {
                throw new Exception($"Failed to create component {componentType}");
            }

            XmlSerializer.DeserializeObject(component, componentElement, idRemap);

            entity.Add(component);
        }
    }
}