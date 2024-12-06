using nkast.Aether.Physics2D.Dynamics;
using System.Numerics;

namespace MalignEngine
{
    public struct BoxCollider2D : IComponent
    {
        internal Fixture Fixture;

        public float Density;
        public float Friction;
        public float Restitution;

        public Vector2 Size;
    }
}