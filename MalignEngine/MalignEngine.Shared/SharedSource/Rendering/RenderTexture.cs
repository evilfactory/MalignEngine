namespace MalignEngine
{
    public class RenderTexture : ITexture
    {
        public uint Width { get; private set; }
        public uint Height { get; private set; }

        public TextureHandle Handle { get; private set; }

        public RenderTexture(uint width, uint height)
        {
            Width = width;
            Height = height;
            Handle = IoCManager.Resolve<RenderingSystem>().CreateTextureHandle();
            Handle.Initialize(width, height, true);
        }

        public void Resize(uint width, uint height)
        {
            Width = width;
            Height = height;
            Handle.Resize(width, height);
        }
    }
}