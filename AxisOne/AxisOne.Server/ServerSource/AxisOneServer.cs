using MalignEngine;
using System.Numerics;

namespace AxisOne;

public class AxisOneServer : EntitySystem, IEventClientConnected
{
    [Dependency]
    protected NetworkingSystem NetworkingSystem = default!;
    [Dependency]
    protected SceneSystem SceneSystem = default!;
    [Dependency]
    protected AssetSystem AssetSystem = default!;

    public void OnClientConnected(NetworkConnection connection)
    {
        Scene playerScene = AssetSystem.GetOfType<Scene>().Where(x => x.Asset.SceneId == "player").First();

        EntityRef player = SceneSystem.LoadScene(playerScene);
        player.Add(new ControllableComponent());

        NetworkingSystem.SpawnEntity(player);
    }
}