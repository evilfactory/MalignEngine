namespace MalignEngine
{
    public class RenderTexture
    {
        public uint Width { get; private set; }
        public uint Height { get; private set; }

        internal RenderTextureHandle handle;

        public RenderTexture(uint width, uint height)
        {
            Width = width;
            Height = height;
            handle = IoCManager.Resolve<RenderingSystem>().CreateRenderTextureHandle(this);
        }

        public void Resize(uint width, uint height)
        {
            Width = width;
            Height = height;
            handle.Resize(width, height);
        }
    }
}