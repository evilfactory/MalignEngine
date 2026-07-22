using System.Numerics;
using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

[Serializable]
public struct ShipSpaceControllerComponent : IComponent
{
    public Entity? Current;
}

[Serializable]
public struct ShipExterior : IComponent { }


public class ShipSpaceController : EntitySystem
{
    [Dependency]
    private IPhysicsSystem2D _physicsSystem = null!;

    public ShipSpaceController(IServiceContainer serviceContainer, INetworkService network) : base(serviceContainer)
    {

    }

    public override void OnUpdate(float deltaTime)
    {
        World.Query(new Query()
            .Include<ShipSpaceControllerComponent>()
            .Include<Transform>()
            .Include<WorldTransform>()
            .Include<PhysicsBody2D>(),
            entity =>
            {
                ref var transform = ref entity.Get<Transform>();
                ref var worldTransform = ref entity.Get<WorldTransform>();
                ref var physicsBody = ref entity.Get<PhysicsBody2D>();
                ref var shipSpaceController = ref entity.Get<ShipSpaceControllerComponent>();

                Vector3 transformPos = transform.Position;
                Vector3 worldTransformPos = worldTransform.Position;
                Entity? shipSpaceControllerCurrent = shipSpaceController.Current;

                bool insideInterior = false;

                _physicsSystem.RayCast((collider, point, normal, fraction) =>
                {
                    if (collider.TryGet(out ComponentRef<ShipInteriorComponent> interiorComponent))
                    {
                        insideInterior = true;
                    }
                    else if (collider.TryGet(out ComponentRef<ShipExteriorComponent> exteriorComponent) && !entity.Has<PhysicsSpaceMember>())
                    {
                        entity.AddOrSet(new ParentOf() { Parent = collider });
                        entity.AddOrSet(new PhysicsSpaceMember() { Space = exteriorComponent.Value.Ship.Get<ShipPhysicsComponent>().Interior });

                        shipSpaceControllerCurrent = collider;

                        transformPos = worldTransformPos - collider.Get<WorldTransform>().Position;
                        insideInterior = true;
                    }

                    return fraction;
                }, _physicsSystem.ToPhysics(entity, transform.Position.ToVector2()), _physicsSystem.ToPhysics(entity, transform.Position.ToVector2()) - Vector2.UnitY * 0.7f);

                shipSpaceController.Current = shipSpaceControllerCurrent;

                transform.Position = transformPos;

                if (!insideInterior && 
                    entity.Has<PhysicsSpaceMember>() &&
                    shipSpaceController.Current != null)
                {
                    Vector3 world = shipSpaceController.Current.Value.Get<WorldTransform>().Position + transform.Position;

                    physicsBody.LinearVelocity = physicsBody.LinearVelocity + shipSpaceController.Current.Value.Get<PhysicsBody2D>().LinearVelocity;
                    entity.Remove<ParentOf>();
                    entity.Remove<PhysicsSpaceMember>();
                    shipSpaceController.Current = null;
                    transform.Position = world;
                }
            });
    }
}