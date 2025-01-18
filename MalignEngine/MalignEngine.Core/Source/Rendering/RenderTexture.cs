namespace MalignEngine
{
    public class RenderTexture : Texture
    {
        public RenderTexture(int width, int height)
        {
            Width = width;
            Height = height;
            Handle = IoCManager.Resolve<IRenderingService>().CreateTextureHandle();
            Handle.Initialize(width, height, true);
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
            Handle.Resize(width, height);
        }
    }
}