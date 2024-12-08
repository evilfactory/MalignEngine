namespace MalignEngine
{
    public abstract class TextureHandle
    {
        public uint Width { get; private set; }
        public uint Height { get; private set; }

        public TextureHandle(uint width, uint height)
        {
            Width = width;
            Height = height;
        }
        public abstract void SubmitData(Color[] data);
        public abstract void SubmitData(System.Drawing.Rectangle bounds, byte[] data);
    }
}