using Microsoft.Xna.Framework;
using Arch.Core.Extensions;
using Genbox.VelcroPhysics.Dynamics;
using Arch.Core;
using Genbox.VelcroPhysics.Definitions;
using World = Genbox.VelcroPhysics.Dynamics.World;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions.Shapes;
using Genbox.VelcroPhysics.Shared;
using Arch.Buffer;

namespace MalignEngine
{
    public class Physics2DSystem : EntitySystem
    {
        private World physicsWorld;

        public Vector2 Gravity
        {
            get => physicsWorld.Gravity;
            set => physicsWorld.Gravity = value;
        }

        public override void Initialize()
        {
            physicsWorld = new World(new Vector2(0, -9.81f));

            EventBus.SubscribeLocalEvent<ComponentAddedEvent, PhysicsBody2D>(PhysicsBodyAdded);
            EventBus.SubscribeLocalEvent<ComponentRemovedEvent, PhysicsBody2D>(PhysicsBodyRemoved);

            EventBus.SubscribeLocalEvent<ComponentAddedEvent, BoxCollider2D>(BoxColliderAdded);
            EventBus.SubscribeLocalEvent<ComponentRemovedEvent, BoxCollider2D>(BoxColliderRemoved);
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
                    physicsBody.Body.Position = position.Position;
                    commandBuffer.Remove<Dirty<Position2D>>(entity);
                }
                else
                {
                    position.Position = physicsBody.Body.Position;
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

            Shape shape = new PolygonShape(new Vertices(new List<Vector2>()
            {
                new Vector2(-boxCollider.Size.X / 2, -boxCollider.Size.Y / 2),
                new Vector2(boxCollider.Size.X / 2, -boxCollider.Size.Y / 2),
                new Vector2(boxCollider.Size.X / 2, boxCollider.Size.Y / 2),
                new Vector2(-boxCollider.Size.X / 2, boxCollider.Size.Y / 2),
            }), boxCollider.Density);

            FixtureDef fixtureDef = new FixtureDef()
            {
                Shape = shape,
                Friction = boxCollider.Friction,
                Restitution = boxCollider.Restitution,
                
            };

            entity.Get<PhysicsBody2D>().Body.CreateFixture(fixtureDef);
        }

        private void BoxColliderRemoved(ComponentRemovedEvent args)
        {
            var entity = args.Entity;
            ref BoxCollider2D boxCollider = ref entity.Get<BoxCollider2D>();

            entity.Get<PhysicsBody2D>().Body.DestroyFixture(boxCollider.Fixture);
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

            BodyDef bodyDef = new BodyDef()
            {
                Position = startPosition,
                Angle = startRotation,
            };

            bodyDef.Type = (BodyType)physicsBody.BodyType;

            Body body = physicsWorld.CreateBody(bodyDef);
            body.Mass = physicsBody.Mass;
            physicsBody.Body = body;
        }

        private void PhysicsBodyRemoved(ComponentRemovedEvent args)
        {
            var entity = args.Entity;
            ref PhysicsBody2D physicsBody = ref entity.Get<PhysicsBody2D>();

            physicsWorld.DestroyBody(physicsBody.Body);
        }
    }
}