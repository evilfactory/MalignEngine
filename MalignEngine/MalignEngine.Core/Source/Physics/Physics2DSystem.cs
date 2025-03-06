using System.Numerics;
using nkast.Aether.Physics2D.Dynamics;
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

            EventService.Get<ComponentEventChannel<ComponentAddedEvent>>().Subscribe<PhysicsBody2D>(PhysicsBodyAdded);
            EventService.Get<ComponentEventChannel<ComponentRemovedEvent>>().Subscribe<PhysicsBody2D>(PhysicsBodyRemoved);
        }

        public override void OnUpdate(float deltaTime)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            physicsWorld.Step(deltaTime);
            stopwatch.Stop();
            EditorPerformanceSystem?.AddElapsedTicks("PhysicsSystem2D", new StopWatchPerformanceLogData(stopwatch.ElapsedTicks));

            var query = EntityManager.World.CreateQuery().WithAll<PhysicsSimId, PhysicsBody2D, Transform>();
            EntityManager.World.Query(in query, (EntityRef entity, ref PhysicsBody2D physicsBody, ref Transform transform, ref PhysicsSimId physicsSimId) =>
            {
                Body body = GetBody(physicsSimId);

                transform.Position = new Vector3(body.Position.X, body.Position.Y, 0f);
                transform.ZAxis = body.Rotation;

                physicsBody.LinearVelocity = new Vector2(body.LinearVelocity.X, body.LinearVelocity.Y);
                physicsBody.AngularVelocity = body.AngularVelocity;
            });
        }

        public void RayCast(Func<FixtureData2D, Vector2, Vector2, float, float> callback, Vector2 start, Vector2 end)
        {
            physicsWorld.RayCast((Fixture fixture, AVector2 point, AVector2 normal, float fraction) =>
            {
                return callback((FixtureData2D)fixture.Tag, new Vector2(point.X, point.Y), new Vector2(point.X, point.Y), fraction);
            }, new AVector2(start.X, start.Y), new AVector2(end.X, end.Y));
        }

        public void SetPosition(in EntityRef entity, Vector2 position)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());
            simBody.Position = new AVector2(position.X, position.Y);
        }

        public void ApplyForce(in EntityRef entity, Vector2 force)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());
            simBody.ApplyForce(new AVector2(force.X, force.Y));
        }

        public void ApplyImpulse(in EntityRef entity, Vector2 impulse)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());
            simBody.ApplyLinearImpulse(new AVector2(impulse.X, impulse.Y));
        }

        public void ApplyTorque(in EntityRef entity, float torque)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());
            simBody.ApplyTorque(torque);
        }

        public void SetLinearVelocity(in EntityRef entity, Vector2 velocity)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());
            simBody.LinearVelocity = new AVector2(velocity.X, velocity.Y);
        }

        public void SetAngularVelocity(in EntityRef entity, float velocity)
        {
            Body simBody = GetBody(entity.Get<PhysicsSimId>());
            simBody.AngularVelocity = velocity;
        }

        public void UpdateFixtures(in EntityRef entity)
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

                fixture.Tag = fixtureData;
                fixture.Restitution = fixtureData.Restitution;
                fixture.Friction = fixtureData.Friction;
                fixture.CollidesWith = (nkast.Aether.Physics2D.Dynamics.Category)fixtureData.CollidesWith;
                fixture.CollisionCategories = (nkast.Aether.Physics2D.Dynamics.Category)fixtureData.CollisionCategories;
            }
        }

        private void PhysicsBodyAdded(EntityRef entity, ComponentAddedEvent args)
        {
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

        private void PhysicsBodyRemoved(EntityRef entity, ComponentRemovedEvent args)
        {
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