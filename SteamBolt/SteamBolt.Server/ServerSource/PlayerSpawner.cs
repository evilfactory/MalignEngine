using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

public class PlayerSpawnerSystem : BaseSystem, IClientSessionStarted
{
    private ServerEntityNetworkSystem _serverEntityNetwork;
    private IAssetService _assetService;
    private SceneSystem _sceneSystem;

    public PlayerSpawnerSystem(IServiceContainer serviceContainer, ServerEntityNetworkSystem serverEntityNetwork, IAssetService assetService, SceneSystem sceneSystem) : base(serviceContainer)
    {
        _serverEntityNetwork = serverEntityNetwork;
        _sceneSystem = sceneSystem;
        _assetService = assetService;
    }

    public void OnClientSessionStarted(NetworkConnection connection, IClientSession session)
    {
        Scene scene = _assetService.FromPath<Scene>("/Content/Player.xml");
        Entity entity = _sceneSystem.Instantiate(scene);
        _serverEntityNetwork.SpawnEntity(entity);
    }
}