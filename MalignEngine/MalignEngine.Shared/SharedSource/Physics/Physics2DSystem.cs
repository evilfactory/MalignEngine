using System.Numerics;
using Arch.Core.Extensions;
using Arch.Core;
using nkast.Aether.Physics2D.Dynamics;
using Arch.Buffer;
using World = nkast.Aether.Physics2D.Dynamics.World;
using AVector2 = nkast.Aether.Physics2D.Common.Vector2;
using nkast.Aether.Physics2D.Collision.Shapes;

namespace MalignEngine
{
    public class Physics2DSystem : EntitySystem
    {
        private World physicsWorld;

        public Vector2 Gravity
        {
            get => new Vector2(physicsWorld.Gravity.X, physicsWorld.Gravity.Y);
            set => physicsWorld.Gravity = new AVector2(value.X, value.Y);
        }

        public override void Initialize()
        {
            physicsWorld = new World(new AVector2(0, -9.81f));

            EventSystem.SubscribeLocalEvent<ComponentAddedEvent, PhysicsBody2D>(PhysicsBodyAdded);
            EventSystem.SubscribeLocalEvent<ComponentRemovedEvent, PhysicsBody2D>(PhysicsBodyRemoved);

            EventSystem.SubscribeLocalEvent<ComponentAddedEvent, BoxCollider2D>(BoxColliderAdded);
            EventSystem.SubscribeLocalEvent<ComponentRemovedEvent, BoxCollider2D>(BoxColliderRemoved);
        }

        public override void Update(float deltaTime)
        {
            physicsWorld.Step(deltaTime);

            CommandBuffer commandBuffer = new CommandBuffer();

            var query = new QueryDescription().WithAll<PhysicsBody2D, Position2D>();
            World.Query(query, (Entity entity, ref PhysicsBody2D physicsBody, ref Position2D position) =>
            {
                if (entity.Has<Dirty<Position2D>>())
                {
                    physicsBody.Body.Position = new AVector2(position.Position.X, position.Position.Y);
                    commandBuffer.Remove<Dirty<Position2D>>(entity);
                }
                else
                {
                    position.Position = new Vector2(physicsBody.Body.Position.X, physicsBody.Body.Position.Y);
                }
            });

            query = new QueryDescription().WithAll<PhysicsBody2D, Rotation2D>();
            World.Query(query, (Entity entity, ref PhysicsBody2D physicsBody, ref Rotation2D rotation) =>
            {
                if (entity.Has<Dirty<Rotation2D>>())
                {
                    physicsBody.Body.Rotation = rotation.Rotation;
                }
                else
                {
                    rotation.Rotation = physicsBody.Body.Rotation;
                    commandBuffer.Remove<Dirty<Rotation2D>>(entity);
                }
            });

            commandBuffer.Playback(World);
        }

        private void BoxColliderAdded(ComponentAddedEvent args)
        {
            var entity = args.Entity;
            ref BoxCollider2D boxCollider = ref entity.Get<BoxCollider2D>();

            boxCollider.Fixture = entity.Get<PhysicsBody2D>().Body.CreateRectangle(boxCollider.Size.X, boxCollider.Size.Y, boxCollider.Density, AVector2.Zero);
        }

        private void BoxColliderRemoved(ComponentRemovedEvent args)
        {
            var entity = args.Entity;
            ref BoxCollider2D boxCollider = ref entity.Get<BoxCollider2D>();

            entity.Get<PhysicsBody2D>().Body.Remove(boxCollider.Fixture);
        }

        private void PhysicsBodyAdded(ComponentAddedEvent args)
        {
            var entity = args.Entity;
            ref PhysicsBody2D physicsBody = ref entity.Get<PhysicsBody2D>();

            Vector2 startPosition = Vector2.Zero;
            if (entity.Has<Position2D>())
            {
                startPosition = entity.Get<Position2D>().Position;
            }
            float startRotation = 0;
            if (entity.Has<Rotation2D>())
            {
                startRotation = entity.Get<Rotation2D>().Rotation;
            }

            Body body = physicsWorld.CreateBody(new AVector2(startPosition.X, startPosition.Y), startRotation, (BodyType)physicsBody.BodyType);
            body.Mass = physicsBody.Mass;
            physicsBody.Body = body;
        }

        private void PhysicsBodyRemoved(ComponentRemovedEvent args)
        {
            var entity = args.Entity;
            ref PhysicsBody2D physicsBody = ref entity.Get<PhysicsBody2D>();

            physicsWorld.Remove(physicsBody.Body);
        }
    }
}