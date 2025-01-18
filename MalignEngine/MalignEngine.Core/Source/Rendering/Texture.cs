namespace MalignEngine;

public interface ITexture
{
    public int Width { get; }
    public int Height { get; }
    public float AspectRatio { get; }
    public TextureHandle Handle { get; }
}

public abstract class Texture : ITexture
{
    public int Width { get; protected set; }
    public int Height { get; protected set; }
    public float AspectRatio
    {
        get
        {
            return (float)Width / Height;
        }
    }
    public TextureHandle Handle { get; protected set; }
}