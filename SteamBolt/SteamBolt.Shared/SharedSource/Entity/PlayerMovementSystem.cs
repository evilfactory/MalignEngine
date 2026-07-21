using System.Numerics;
using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

public class PlayerMoveNetMessage : NetMessage
{
    public NetEntityId EntityId;
    public Vector2 Position;

    public override void Serialize(IWriteMessage message)
    {
        message.WriteUInt32(EntityId.Value);
        message.WriteSingle(Position.X);
        message.WriteSingle(Position.Y);
    }

    public override void Deserialize(IReadMessage message)
    {
        EntityId = new NetEntityId() { Value = message.ReadUInt32() };
        Position = new Vector2(message.ReadSingle(), message.ReadSingle());
    }
}

public class PlayerMovementSystem : EntitySystem
{
    private INetworkService _network;

    [Dependency(optional: true)]
    private ServerSessionSystem _sessionSystem = null!;

    [Dependency(optional: true)]
    private ClientSessionSystem _clientSession = null!;

    [Dependency]
    private EntityNetworkSystem _entityNetwork = null!;

    [Dependency]
    private IPhysicsSystem2D _physicsSystem = null!;

    public PlayerMovementSystem(IServiceContainer serviceContainer, INetworkService network) : base(serviceContainer)
    {
        _network = network;

        if (_network.Server != null)
        {
            _network.Server.Register<PlayerMoveNetMessage>(ServerReceiveMoveNetMessage);
        }

        if (_network.Client != null)
        {
            _network.Client.Register<PlayerMoveNetMessage>(ClientReceiveMoveNetMessage);
        }
    }

    private void ServerReceiveMoveNetMessage(NetworkConnection connection, PlayerMoveNetMessage message)
    {
        if (_entityNetwork.TryGetEntityFromId(message.EntityId, out Entity entity))
        {
            entity.Get<Transform>().Position = message.Position.ToVector3();
        }
    }

    private void ClientReceiveMoveNetMessage(PlayerMoveNetMessage message)
    {
        if (_entityNetwork.TryGetEntityFromId(message.EntityId, out Entity entity))
        {
            if (!entity.Has<OwnerComponent>() || entity.Get<OwnerComponent>().ClientId != _clientSession.ClientId)
            {
                entity.Get<Transform>().Position = message.Position.ToVector3();
            }
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        World.Query(new Query()
            .Include<PlayerMovementComponent>()
            .Include<PlayerInputComponent>()
            .Include<Transform>()
            .Include<WorldTransform>()
            .Include<PhysicsBody2D>(),
            entity =>
        {
            ref var input = ref entity.Get<PlayerInputComponent>();
            ref var movement = ref entity.Get<PlayerMovementComponent>();
            ref var transform = ref entity.Get<Transform>();
            ref var worldTransform = ref entity.Get<WorldTransform>();
            ref var body = ref entity.Get<PhysicsBody2D>();

            transform.SetRotation2D(0f);

            bool grounded = false;

            _physicsSystem.RayCast((collider, point, normal, fraction) =>
            {
                if (collider != entity)
                {
                    grounded = true;
                }

                return fraction;
            }, worldTransform.Position.ToVector2(), worldTransform.Position.ToVector2() - Vector2.UnitY * 0.7f);

            float inputX = 0;

            if (input.Left) 
            { 
                inputX -= 1; 
            }
            if (input.Right) 
            { 
                inputX += 1; 
            }

            float targetSpeed = inputX * movement.MoveSpeed;

            float acceleration = grounded ? movement.GroundAcceleration : movement.AirAcceleration;
            float deceleration = grounded ? movement.GroundDeceleration : movement.AirDeceleration;

            float current = body.LinearVelocity.X;

            if (MathF.Abs(targetSpeed) > 0.01f)
            {
                current = MoveTowards(current, targetSpeed, acceleration * deltaTime);
            }
            else
            {
                current = MoveTowards(current, 0, deceleration * deltaTime);
            }

            body.LinearVelocity.X = current;

            if (grounded && input.Up)
            {
                body.LinearVelocity.Y = movement.JumpVelocity;
            }

            if (!input.Up && body.LinearVelocity.Y > 0)
            {
                body.LinearVelocity.Y *= movement.JumpCutMultiplier;
            }

            if (_clientSession != null && entity.Has<OwnerComponent>() && entity.Get<OwnerComponent>().ClientId == _clientSession.ClientId)
            {
                if (_network.Client != null)
                {
                    _network.Client.Send(new PlayerMoveNetMessage 
                    { 
                        EntityId = entity.Get<NetEntityId>(), 
                        Position = entity.Get<Transform>().Position.ToVector2() 
                    });
                }
            }

            if (_network.Server != null)
            {
                _entityNetwork.BroadcastSynced(new PlayerMoveNetMessage
                {
                    EntityId = entity.Get<NetEntityId>(),
                    Position = entity.Get<Transform>().Position.ToVector2()
                });
            }
        });
    }

    private static float MoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
        {
            return target;
        }

        return current + MathF.Sign(target - current) * maxDelta;
    }
}