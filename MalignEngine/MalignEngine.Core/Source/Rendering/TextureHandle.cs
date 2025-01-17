namespace MalignEngine
{
    public abstract class TextureHandle
    {
        public uint Width { get; protected set; }
        public uint Height { get; protected set; }

        public TextureHandle() { }
        public abstract void Initialize(uint width, uint height, bool frameBuffer = false);
        public abstract void Resize(uint width, uint height);
        public abstract void SubmitData(Color[] data);
        public abstract void SubmitData(System.Drawing.Rectangle bounds, byte[] data);
    }
}