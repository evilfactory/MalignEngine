using System.Numerics;
using Arch.Core.Extensions;
using Arch.Core;
using nkast.Aether.Physics2D.Dynamics;
using Arch.Buffer;
using World = nkast.Aether.Physics2D.Dynamics.World;
using AVector2 = nkast.Aether.Physics2D.Common.Vector2;
using nkast.Aether.Physics2D.Collision.Shapes;
using System.Diagnostics;

namespace MalignEngine
{
    public class Physics2DSystem : EntitySystem
    {
        [Dependency(true)]
        protected EditorPerformanceSystem EditorPerformanceSystem = default!;

        private World physicsWorld;

        public Vector2 Gravity
        {
            get => new Vector2(physicsWorld.Gravity.X, physicsWorld.Gravity.Y);
            set => physicsWorld.Gravity = new AVector2(value.X, value.Y);
        }

        public override void OnInitialize()
        {
            physicsWorld = new World(new AVector2(0, -9.81f));

            EntityEventSystem.SubscribeLocalEvent<ComponentAddedEvent, PhysicsBody2D>(PhysicsBodyAdded);
            EntityEventSystem.SubscribeLocalEvent<ComponentRemovedEvent, PhysicsBody2D>(PhysicsBodyRemoved);

            EntityEventSystem.SubscribeLocalEvent<ComponentAddedEvent, BoxCollider2D>(BoxColliderAdded);
            EntityEventSystem.SubscribeLocalEvent<ComponentRemovedEvent, BoxCollider2D>(BoxColliderRemoved);
        }

        public override void OnUpdate(float deltaTime)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            physicsWorld.Step(deltaTime);
            stopwatch.Stop();
            EditorPerformanceSystem?.AddElapsedTicks("Physics2DSystem", stopwatch.ElapsedTicks);

            CommandBuffer commandBuffer = new CommandBuffer();

            var query = new QueryDescription().WithAll<PhysicsBody2D, Transform>();
            World.Query(in query, (Entity entity, ref PhysicsBody2D physicsBody, ref Transform transform) =>
            {
                if (entity.Has<Dirty<Transform>>())
                {
                    physicsBody.Body.Position = new AVector2(transform.Position.X, transform.Position.Y);
                    physicsBody.Body.Rotation = transform.ZAxis;
                }
                else
                {
                    transform.Position = new Vector3(physicsBody.Body.Position.X, physicsBody.Body.Position.Y, 0f);
                    transform.ZAxis = physicsBody.Body.Rotation;
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
            float startRotation = 0;

            if (entity.Has<Transform>())
            {
                startPosition = entity.Get<Transform>().Position.ToVector2();
                startRotation = entity.Get<Transform>().ZAxis;
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