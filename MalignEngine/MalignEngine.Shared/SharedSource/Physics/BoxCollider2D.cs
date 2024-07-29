using Genbox.VelcroPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace MalignEngine
{
    [RegisterComponent]
    public struct BoxCollider2D
    {
        internal Fixture Fixture;

        public float Density;
        public float Friction;
        public float Restitution;

        public Vector2 Size;
    }
}