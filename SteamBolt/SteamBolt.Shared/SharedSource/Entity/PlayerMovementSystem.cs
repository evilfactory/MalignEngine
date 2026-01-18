using System.Numerics;
using MalignEngine;

namespace SteamBolt;

public class PlayerMovementSystem : EntitySystem
{
    public PlayerMovementSystem(IServiceContainer serviceContainer) : base(serviceContainer)
    {
    }

    public override void OnUpdate(float deltaTime)
    {
        World.Query(new Query()
            .Include<PlayerMovementComponent>()
            .Include<PlayerInputComponent>(), 
            entity =>
        {
            ref var input = ref entity.Get<PlayerInputComponent>();
            ref var movement = ref entity.Get<PlayerMovementComponent>();

            Vector2 force = Vector2.Zero;

            if (input.Left)
            {
                force.X -= movement.MoveSpeed;
            }

            if (input.Right)
            {
                force.X += movement.MoveSpeed;
            }

            if (input.Up && movement.IsGrounded)
            {
                force.Y -= movement.JumpForce;
                movement.IsGrounded = false;
            }

            if (force != Vector2.Zero)
            {
                entity.AddOrSet(new ApplyForce
                {
                    Force = force
                });
            }
        });
    }
}