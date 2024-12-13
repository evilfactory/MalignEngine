using Arch.Core;

namespace MalignEngine;

public class Scene : IAsset
{
    private Action<Func<EntityRef>, WorldRef> loadAction;

    public Scene(Action<Func<EntityRef>, WorldRef> loadAction)
    {
        this.loadAction = loadAction;
    }

    public void Load(Func<EntityRef> createEntityFunc, WorldRef world)
    {
        loadAction(createEntityFunc, world);
    }
}