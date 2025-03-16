using System.Numerics;

namespace MalignEngine
{
    public struct Light2D : IComponent
    {
        public Color Color;
        public Texture2D Texture;
        
        public bool ShadowCasting;
        public bool ShadowGeometryDirty;

        public Renderer2D.Vertex[] ShadowGeometry;
    }

    public struct GlobalLight2D : IComponent
    {
        public Color Color;
    }
}