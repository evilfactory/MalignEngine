namespace MalignEngine
{
    /// <summary>
    /// A texture handle is the way for the engine to interact with the underlying graphics API.
    /// </summary>
    public abstract class TextureHandle
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public TextureHandle() { }
        public abstract void Initialize(int width, int height, bool frameBuffer = false);
        public abstract void Resize(int width, int height);
        public abstract void SubmitData(Color[] data);
        public abstract void SubmitData(System.Drawing.Rectangle bounds, byte[] data);
    }
}