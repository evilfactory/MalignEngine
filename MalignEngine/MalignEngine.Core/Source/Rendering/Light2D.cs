namespace MalignEngine
{
    public struct Light2D : IComponent
    {
        public Color Color;
        public Texture2D Texture;
    }

    public struct GlobalLight2D : IComponent
    {
        public Color Color;
    }
}