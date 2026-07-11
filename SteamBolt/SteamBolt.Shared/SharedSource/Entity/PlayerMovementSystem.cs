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
            .Include<Transform>(),
            entity =>
        {
            ref var input = ref entity.Get<PlayerInputComponent>();
            ref var movement = ref entity.Get<PlayerMovementComponent>();
            ref var transform = ref entity.Get<Transform>();

            Vector2 force = Vector2.Zero;

            if (input.Left)
            {
                force.X -= movement.MoveSpeed;
            }

            if (input.Right)
            {
                force.X += movement.MoveSpeed;
            }

            bool grounded = false;
            _physicsSystem.RayCast((collider, point, normal, fraction) =>
            {
                if (collider != entity)
                {
                    grounded = true;
                }

                return 1f;
            }, transform.Position.ToVector2(), transform.Position.ToVector2() - Vector2.UnitY);

            if (input.Up && grounded)
            {
                force.Y += movement.JumpForce;
            }

            if (force != Vector2.Zero)
            {
                entity.AddOrSet(new ApplyForce
                {
                    Force = force
                });
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
}