using MalignEngine;
using System.Numerics;

namespace SteamBolt;

public class ShipSystem : EntitySystem
{
    [Dependency(optional: true)]
    protected IInputService InputService = null!;

    public ShipSystem(IServiceContainer serviceContainer) : base(serviceContainer)
    {
    }

    public override void OnUpdate(float deltaTime)
    {
        if (InputService == null) { return; }

        World.Query(new Query().Include<ShipPhysicsComponent>(), entity =>
        {
            ref ShipPhysicsComponent ship = ref entity.Get<ShipPhysicsComponent>();

            ref PhysicsBody2D physics = ref ship.Exterior.Get<PhysicsBody2D>();

            Vector2 force = Vector2.Zero;

            if (InputService.Keyboard.IsKeyPressed(Key.UpArrow))
            {
                force += Vector2.UnitY;
            }

            if (InputService.Keyboard.IsKeyPressed(Key.DownArrow))
            {
                force += -Vector2.UnitY;
            }

            if (InputService.Keyboard.IsKeyPressed(Key.RightArrow))
            {
                force += Vector2.UnitX;
            }

            if (InputService.Keyboard.IsKeyPressed(Key.LeftArrow))
            {
                force += -Vector2.UnitX;
            }

            ship.Exterior.AddOrSet(new ApplyForce() { Force = force * 50f });
        });
    }
}