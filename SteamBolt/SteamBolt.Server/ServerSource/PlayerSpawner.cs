using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

public class PlayerSpawnerSystem : BaseSystem, IClientSessionStarted
{
    private EntityNetworkSystem _entityNetworkSystem;
    private IAssetService _assetService;
    private SceneSystem _sceneSystem;

    public PlayerSpawnerSystem(IServiceContainer serviceContainer, EntityNetworkSystem entityNetworkSystem, IAssetService assetService, SceneSystem sceneSystem) : base(serviceContainer)
    {
        _entityNetworkSystem = entityNetworkSystem;
        _sceneSystem = sceneSystem;
        _assetService = assetService;
    }

    public void OnClientSessionStarted(NetworkConnection connection, IClientSession session)
    {
        _entityNetworkSystem.SyncEntities(connection);

        Scene scene = _assetService.FromPath<Scene>("/Content/Player.xml");
        Entity entity = _sceneSystem.Instantiate(scene);
        entity.AddOrSet(new OwnerComponent() { ClientId = session.ClientId });
        _entityNetworkSystem.SpawnEntity(entity);
    }
}