using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;
using System.Numerics;
using AVector2 = nkast.Aether.Physics2D.Common.Vector2;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;
using Vertices = nkast.Aether.Physics2D.Common.Vertices;

namespace MalignEngine;

internal struct PhysicsSim : IComponent
{
    public Body Body;
    public int LastFixtureHash;
}

public interface IPhysicsSystem2D : IService
{
    Vector2 Gravity { get; set; }
    void RayCast(Func<Entity, Vector2, Vector2, float, float> callback, Vector2 start, Vector2 end);
    void QueryAABB(Func<Entity, bool> action, Vector2 min, Vector2 max);
}

public class PhysicsSystem2D : IPhysicsSystem2D, IPostUpdate
{
    [Dependency]
    protected IPerformanceProfiler? _performanceProfiler;

    private readonly IEntityManager _entityManager;
    private readonly PhysicsWorld _physicsWorld;

    public Vector2 Gravity
    {
        get => new Vector2(_physicsWorld.Gravity.X, _physicsWorld.Gravity.Y);
        set => _physicsWorld.Gravity = new AVector2(value.X, value.Y);
    }

    public PhysicsSystem2D(IEventService eventService, IEntityManager entityManager)
    {
        _physicsWorld = new PhysicsWorld(new AVector2(0, -9.81f));
        _entityManager = entityManager;
    }

    public void RayCast(Func<Entity, Vector2, Vector2, float, float> callback, Vector2 start, Vector2 end)
    {
        _physicsWorld.RayCast((Fixture fixture, AVector2 point, AVector2 normal, float fraction) =>
        {
            return callback((Entity)fixture.Tag, new Vector2(point.X, point.Y), new Vector2(point.X, point.Y), fraction);
        }, new AVector2(start.X, start.Y), new AVector2(end.X, end.Y));
    }

    public void QueryAABB(Func<Entity, bool> action, Vector2 min, Vector2 max)
    {
        _physicsWorld.QueryAABB((Fixture fixture) =>
        {
            return action((Entity)fixture.Tag);
        }, new nkast.Aether.Physics2D.Collision.AABB(new AVector2(min.X, min.Y), new AVector2(max.X, max.Y)));
    }

    private void UpdateFixtures(Entity entity, ref PhysicsBody2D physicsBody, ref PhysicsSim physicsSim)
    {
        foreach (var fixture in physicsSim.Body.FixtureList.ToList())
        {
            physicsSim.Body.Remove(fixture);
        }

        if (physicsBody.Fixtures == null) { return; }

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

                fixture = physicsSim.Body.CreateFixture(new PolygonShape(new Vertices(vertices), fixtureData.Density));
            }
            else if (fixtureData.Shape.Type == ShapeType.Circle)
            {
                fixture = physicsSim.Body.CreateFixture(new CircleShape(fixtureData.Shape.Radius / 2f, fixtureData.Density));
            }
            else
            {
                throw new Exception("Shape type not supported");
            }

            fixture.Tag = entity;
            fixture.Restitution = fixtureData.Restitution;
            fixture.Friction = fixtureData.Friction;
            fixture.CollidesWith = (nkast.Aether.Physics2D.Dynamics.Category)fixtureData.CollidesWith;
            fixture.CollisionCategories = (nkast.Aether.Physics2D.Dynamics.Category)fixtureData.CollisionCategories;
        }
    }

    public void OnPostUpdate(float deltaTime)
    {
        // Add physics bodies to simulation
        _entityManager.World.Query(new Query().Include<PhysicsBody2D>().Exclude<PhysicsSim>(), (Entity entity) =>
        {
            ref PhysicsBody2D physicsBody2D = ref entity.Get<PhysicsBody2D>();

            Body body = _physicsWorld.CreateBody(bodyType: (BodyType)physicsBody2D.BodyType);
            entity.AddOrSet(new PhysicsSim() { Body = body, LastFixtureHash = 0 });
        });

        using (_performanceProfiler?.BeginSample("PhysicsSystem2D.PutTransformations"))
        {
            // Put transformations into simulation
            _entityManager.World.Query(new Query().WithAll<PhysicsBody2D, PhysicsSim>(), (Entity entity) =>
            {
                ref PhysicsBody2D physicsBody2D = ref entity.Get<PhysicsBody2D>();
                ref PhysicsSim physicsSim = ref entity.Get<PhysicsSim>();

                physicsSim.Body.LinearVelocity = new AVector2(physicsBody2D.LinearVelocity.X, physicsBody2D.LinearVelocity.Y);
                physicsSim.Body.AngularVelocity = physicsBody2D.AngularVelocity;

                if (physicsBody2D.Mass != physicsSim.Body.Mass)
                {
                    physicsBody2D.Mass = physicsSim.Body.Mass;
                }

                if (physicsSim.LastFixtureHash != HashCode.Combine(physicsBody2D.Fixtures))
                {
                    UpdateFixtures(entity, ref physicsBody2D, ref physicsSim);
                    physicsSim.LastFixtureHash = HashCode.Combine(physicsBody2D.Fixtures);
                }

                if (entity.TryGet(out ComponentRef<Transform> transform))
                {
                    physicsSim.Body.Position = new AVector2(transform.Value.Position.X, transform.Value.Position.Y);
                    physicsSim.Body.Rotation = transform.Value.EulerAngles.Z;
                }

                if (entity.TryGet(out ComponentRef<ApplyForce> applyForce))
                {
                    physicsSim.Body.ApplyForce(new AVector2(applyForce.Value.Force.X, applyForce.Value.Force.Y));
                    applyForce.Value.Force = Vector2.Zero;
                }
            });
        }

        using (_performanceProfiler?.BeginSample("PhysicsSystem2D.Tick"))
        {
            _physicsWorld.Step(deltaTime);
        }

        using (_performanceProfiler?.BeginSample("PhysicsSystem2D.BringTransformations"))
        {
            // Bring transformations from simulation
            _entityManager.World.Query(new Query().WithAll<PhysicsBody2D, PhysicsSim, Transform>(), (Entity entity) =>
            {
                ref PhysicsBody2D physicsBody2D = ref entity.Get<PhysicsBody2D>();
                ref PhysicsSim physicsSim = ref entity.Get<PhysicsSim>();

                physicsBody2D.LinearVelocity = new Vector2(physicsSim.Body.LinearVelocity.X, physicsSim.Body.LinearVelocity.Y);
                physicsBody2D.AngularVelocity = physicsSim.Body.AngularVelocity;

                if (physicsBody2D.Mass != physicsSim.Body.Mass)
                {
                    physicsSim.Body.Mass = physicsBody2D.Mass;
                }

                if (entity.TryGet(out ComponentRef<Transform> transform))
                {
                    transform.Value.Position = new Vector3(physicsSim.Body.Position.X, physicsSim.Body.Position.Y, transform.Value.Position.Z);
                    transform.Value.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, physicsSim.Body.Rotation);
                }
            });
        }
    }
}