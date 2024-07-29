using Genbox.VelcroPhysics.Dynamics;

namespace MalignEngine
{
    public enum PhysicsBodyType
    {
        Static,
        Kinematic,
        Dynamic
    }

    [RegisterComponent]
    public struct PhysicsBody2D
    {
        public PhysicsBodyType BodyType;

        public float Mass;

        internal Body Body;
    }
}