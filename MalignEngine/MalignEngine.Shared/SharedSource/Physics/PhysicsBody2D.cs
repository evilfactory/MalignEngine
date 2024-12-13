
using nkast.Aether.Physics2D.Dynamics;
using System.Numerics;

namespace MalignEngine
{
    public enum PhysicsBodyType
    {
        Static,
        Kinematic,
        Dynamic
    }

    public struct PhysicsBody2D : IComponent
    {
        public PhysicsBodyType BodyType;
        public float Mass;

        public Vector2 LinearVelocity;
        public float AngularVelocity;

        public FixtureData2D[] Fixtures;
    }

    internal struct PhysicsSimId : IComponent
    {
        public uint Id;
    }
}