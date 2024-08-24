namespace MalignEngine
{
    public abstract class RenderTextureHandle
    {
        public uint Width { get; protected set; }
        public uint Height { get; protected set; }

        public RenderTextureHandle(uint width, uint height)
        {
            Width = width;
            Height = height;
        }

        public abstract void Resize(uint width, uint height);
    }
}