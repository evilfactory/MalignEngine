using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace MalignEngine;

public class EntityIdRemap
{
    public Dictionary<int, Entity> IdRemap
    {
        get; private set;
    }

    public EntityIdRemap()
    {
        IdRemap = new Dictionary<int, Entity>();
    }

    public void AddEntity(int id, Entity entity)
    {
        IdRemap.Add(id, entity);
    }

    public Entity GetEntity(int id)
    {
        return IdRemap[id];
    }

    public void Clear()
    {
        IdRemap.Clear();
    }

}

public class EntitySerializer : IService
{
    private Dictionary<string, Type> components;

    private XmlSerializer _xmlSerializer;

    public EntitySerializer(XmlSerializer xmlSerializer)
    {
        _xmlSerializer = xmlSerializer;

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

    public void SerializeEntity(Entity entity, XElement data, bool saveAll = false)
    {
        data.SetAttributeValue("Id", entity.Id.ToString());

        foreach (IComponent component in entity.GetComponents())
        {
            if (component.GetType().GetCustomAttribute<SerializableAttribute>() == null)
            {
                continue;
            }

            XElement componentElement = new XElement(component.GetType().Name);

            _xmlSerializer.SerializeObject(component, componentElement, saveAll);

            data.Add(componentElement);
        }
    }

    public void DeserializeEntity(Entity entity, XElement data, EntityIdRemap idRemap)
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

            _xmlSerializer.DeserializeObject(component, componentElement, idRemap);

            entity.AddOrSet(component);
        }
    }
}