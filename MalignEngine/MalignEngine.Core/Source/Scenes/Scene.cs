using Arch.Core;
using nkast.Aether.Physics2D.Dynamics;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine;

/// <summary>
/// A scene holds a list of entities that can be used to create copies of the scene.
/// </summary>
public class Scene : XmlAsset<Scene>, IAssetWithId
{
    public string? AssetId { get; private set; }
    public EntityRef Root { get; private set; }
    public WorldRef SceneWorld { get; private set; }

    public Scene()
    {
        SceneWorld = new WorldRef();

        Root = SceneWorld.CreateEntity();
    }

    public override void Load(XElement element)
    {
        // Destroy all entities
        foreach (var entity in SceneWorld.AllEntities())
        {
            SceneWorld.Destroy(entity);
        }

        AssetId = element.Attribute("Id")?.Value ?? null;

        EntityIdRemap remap = new EntityIdRemap();

        List<(EntityRef, XElement)> entities = new List<(EntityRef, XElement)>();

        // Create all entities first
        foreach (var entityElement in element.Elements())
        {
            EntityRef entity = SceneWorld.CreateEntity();
            remap.AddEntity(int.Parse(entityElement.Attribute("Id")?.Value), entity);
            entities.Add((entity, entityElement));
        }

        // Then deserialize them
        foreach (var (entity, entityElement) in entities)
        {
            EntitySerializer.DeserializeEntity(entity, entityElement, remap);
        }

        // The first entity is the root
        Root = entities[0].Item1;
    }

    public override void Save(XElement element)
    {
        // Go through all entities in the scene and serialize them

        element.SetAttributeValue("Id", AssetId);

        foreach (var entity in SceneWorld.AllEntities())
        {
            XElement entityElement = new XElement("Entity");
            entityElement.SetAttributeValue("Id", entity.Id.ToString());

            EntitySerializer.SerializeEntity(entity, entityElement);

            element.Add(entityElement);
        }
    }

    public void CopyEntities(EntityRef[] entities)
    {
        EntityIdRemap idRemap = new EntityIdRemap();

        EntityRef[] copyEntities = new EntityRef[entities.Length];

        for (int i = 0; i < entities.Length; i++)
        {
            copyEntities[i] = SceneWorld.CreateEntity();

            idRemap.AddEntity(entities[i].Id, copyEntities[i]);
        }

        for (int i = 0; i < entities.Length; i++)
        {
            SceneSystem.CopyEntity(entities[i], copyEntities[i], idRemap);
        }
    }
}