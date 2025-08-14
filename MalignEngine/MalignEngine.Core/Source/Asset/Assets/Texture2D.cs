namespace MalignEngine;

public class Texture2D : IAsset
{
    public ITextureResource Resource { get; private set; }

    public int Width => Resource.Width;
    public int Height => Resource.Height;

    public Texture2D(ITextureResource resource)
    {
        this.Resource = resource;
    }
}
