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
    protected AssetService AssetService = default!;

    public void OnClientConnected(NetworkConnection connection)
    {
        Scene playerScene = AssetService.GetFromId<Scene>("player");

        EntityRef player = SceneSystem.Instantiate(playerScene);
        player.Add(new ControllableComponent());

        NetworkingSystem.SpawnEntity(player);
    }
}