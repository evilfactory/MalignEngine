namespace MalignEngine;

public struct SceneComponent { }

public class SceneSystem : EntitySystem
{
    public override void OnInitialize()
    {

    }

    public void LoadScene(Scene scene)
    {
        EntityRef root = EntityManager.World.CreateEntity();
        root.Add(new Transform());
        root.Add(new NameComponent("Scene"));

        EntityRef createEntityFunc()
        {
            EntityRef entity = EntityManager.World.CreateEntity();
            entity.Add(new ParentOf { Parent = root });

            return entity;
        }

        scene.Load(createEntityFunc, EntityManager.World);
    }
}