using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

public class PlayerCameraSystem : EntitySystem
{
    [Dependency]
    private ClientSessionSystem _session = null!;

    public PlayerCameraSystem(IServiceContainer serviceContainer) : base(serviceContainer)
    {
    }

    public override void OnUpdate(float deltaTime)
    {
        EntityManager.Query(new Query().Include<OrthographicCamera>().Include<OwnerComponent>(), entity =>
        {
            entity.Get<OrthographicCamera>().IsMain = entity.Get<OwnerComponent>().ClientId == _session.ClientId;
        });
    }
}