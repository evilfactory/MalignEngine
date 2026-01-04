using nkast.Aether.Physics2D.Dynamics;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine;

/// <summary>
/// A scene holds a list of entities that can be used to create copies of the scene.
/// </summary>
public class Scene : IAsset
{
    public string SceneId { get; private set; }
    public Entity Root { get; private set; }
    public World SceneWorld { get; private set; }

    public XElement? OriginalElement { get; set; } // Remove in the future

    public Scene(string sceneId)
    {
        SceneId = sceneId;

        SceneWorld = new World();
        Root = SceneWorld.CreateEntity();
    }

    public Scene(string sceneId, Entity[] entities)
    {
        SceneId = sceneId;
        SceneWorld = new World();

        Entity[] copied = CopyEntities(entities);
        Root = copied[0];
    }

    public Scene (string sceneId, World world, Entity root)
    {
        SceneId = sceneId;
        SceneWorld = world;
        Root = root;
    }

    public Entity[] CopyEntities(Entity[] entities)
    {
        EntityIdRemap idRemap = new EntityIdRemap();

        Entity[] copyEntities = new Entity[entities.Length];

        for (int i = 0; i < entities.Length; i++)
        {
            copyEntities[i] = SceneWorld.CreateEntity();

            idRemap.AddEntity(entities[i].Id, copyEntities[i]);
        }

        for (int i = 0; i < entities.Length; i++)
        {
            SceneSystem.CopyEntity(entities[i], copyEntities[i], idRemap);
        }

        return copyEntities;
    }
}