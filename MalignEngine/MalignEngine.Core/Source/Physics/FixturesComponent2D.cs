using nkast.Aether.Physics2D.Dynamics;

namespace MalignEngine
{
    public class FixtureData2D
    {
        public float Density;
        public float Friction;
        public float Restitution;

        public IPhysicsShape2D Shape;

        public FixtureData2D(IPhysicsShape2D shape, float density, float friction, float restitution)
        {
            Shape = shape;
            Density = density;
            Friction = friction;
            Restitution = restitution;
        }
    }
}