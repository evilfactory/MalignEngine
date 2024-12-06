
using nkast.Aether.Physics2D.Dynamics;

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

        internal Body Body;
    }
}