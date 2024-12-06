using System.Numerics;
using Arch.Core.Extensions;
using Arch.Core;
using nkast.Aether.Physics2D.Dynamics;
using Arch.Buffer;
using World = nkast.Aether.Physics2D.Dynamics.World;
using AVector2 = nkast.Aether.Physics2D.Common.Vector2;
using Vertices = nkast.Aether.Physics2D.Common.Vertices;
using nkast.Aether.Physics2D.Collision.Shapes;
using System.Diagnostics;

namespace MalignEngine
{
    public class PhysicsSystem2D : EntitySystem
    {
        private Dictionary<PhysicsSimId, Body> simBodies = new Dictionary<PhysicsSimId, Body>();

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
        }

        public override void OnUpdate(float deltaTime)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            physicsWorld.Step(deltaTime);
            stopwatch.Stop();
            EditorPerformanceSystem?.AddElapsedTicks("PhysicsSystem2D", stopwatch.ElapsedTicks);

            var query = new QueryDescription().WithAll<PhysicsSimId, PhysicsBody2D, Transform>();
            World.Query(in query, (Entity entity, ref PhysicsBody2D physicsBody, ref Transform transform, ref PhysicsSimId physicsSimId) =>
            {
                Body body = GetBody(physicsSimId);

                transform.Position = new Vector3(body.Position.X, body.Position.Y, 0f);
                transform.ZAxis = body.Rotation;

                physicsBody.LinearVelocity = new Vector2(body.LinearVelocity.X, body.LinearVelocity.Y);
                physicsBody.AngularVelocity = body.AngularVelocity;
            });
        }

        public void ApplyForce(in Entity entity, Vector2 force)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());
            simBody.ApplyForce(new AVector2(force.X, force.Y));
        }

        public void ApplyImpulse(in Entity entity, Vector2 impulse)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());
            simBody.ApplyLinearImpulse(new AVector2(impulse.X, impulse.Y));
        }

        public void ApplyTorque(in Entity entity, float torque)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());
            simBody.ApplyTorque(torque);
        }

        public void SetLinearVelocity(in Entity entity, Vector2 velocity)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());
            simBody.LinearVelocity = new AVector2(velocity.X, velocity.Y);
        }

        public void SetAngularVelocity(in Entity entity, float velocity)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());
            simBody.AngularVelocity = velocity;
        }

        public void UpdateFixtures(in Entity entity)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());

            ref PhysicsBody2D physicsBody = ref entity.Get<PhysicsBody2D>();

            foreach (var fixture in simBody.FixtureList)
            {
                simBody.Remove(fixture);
            }

            foreach (var fixtureData in physicsBody.Fixtures)
            {
                Fixture fixture;
                if (fixtureData.Shape.Type == ShapeType.Polygon)
                {
                    AVector2[] vertices = new AVector2[fixtureData.Shape.Vertices.Length];
                    for (int i = 0; i < fixtureData.Shape.Vertices.Length; i++)
                    {
                        vertices[i] = new AVector2(fixtureData.Shape.Vertices[i].X, fixtureData.Shape.Vertices[i].Y);
                    }

                    fixture = simBody.CreateFixture(new PolygonShape(new Vertices(vertices), fixtureData.Density));
                }
                else if (fixtureData.Shape.Type == ShapeType.Circle)
                {
                    fixture = simBody.CreateFixture(new CircleShape(fixtureData.Shape.Radius / 2f, fixtureData.Density));
                }
                else
                {
                    throw new Exception("Shape type not supported");
                }

                fixture.Restitution = fixtureData.Restitution;
                fixture.Friction = fixtureData.Friction;
            }
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

            // Create Body
            Body body = physicsWorld.CreateBody(new AVector2(startPosition.X, startPosition.Y), startRotation, (BodyType)physicsBody.BodyType);
            body.Mass = physicsBody.Mass;

            uint id = GetFreeId();
            entity.Add(new PhysicsSimId() { Id = id });
            simBodies.Add(new PhysicsSimId() { Id = id }, body);

            // Create Fixtures
            UpdateFixtures(entity);
        }

        private void PhysicsBodyRemoved(ComponentRemovedEvent args)
        {
            var entity = args.Entity;
            ref PhysicsBody2D physicsBody = ref entity.Get<PhysicsBody2D>();

            PhysicsSimId id = entity.Get<PhysicsSimId>();

            physicsWorld.Remove(GetBody(id));
            simBodies.Remove(id);
        }

        private uint GetFreeId()
        {
            uint id = 1;
            while (simBodies.ContainsKey(new PhysicsSimId() { Id = id }))
            {
                id++;
                if (id == uint.MaxValue)
                {
                    throw new Exception("No free id found");
                }
            }
            return id;
        }

        private Body GetBody(PhysicsSimId id)
        {
            if (simBodies.TryGetValue(id, out Body body))
            {
                return body;
            }
            else
            {
                throw new Exception("Body not found");
            }
        }
    }
}