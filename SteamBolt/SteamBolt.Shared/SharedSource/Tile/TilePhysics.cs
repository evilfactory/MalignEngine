using MalignEngine;

namespace SteamBolt;

public struct ShipPhysicsComponent : IComponent
{
    public Entity Interior;
    public Entity Exterior;
}

public class TilePhysics : EntitySystem
{
    public TilePhysics(IServiceContainer serviceContainer) : base(serviceContainer)
    {
    }

    public override void OnUpdate(float deltaTime)
    {

    }
}