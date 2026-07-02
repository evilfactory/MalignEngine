
namespace MalignEngine;

public class TextureAssetLoader : IService, IAssetLoader
{
    public IReadOnlyCollection<Type> AssetTypes => [typeof(Texture2D)];

    private IRenderingAPI _renderingAPI;

    public TextureAssetLoader(IRenderingAPI renderingAPI)
    {
        _renderingAPI = renderingAPI;
    }

    public IAsset Load(Stream stream)
    {
        ITextureDescriptor descriptor = TextureLoader.Load(stream);
        ITextureResource resource = _renderingAPI.CreateTexture(descriptor);
        Texture2D texture = new Texture2D(resource);
        return texture;
    }

    public void Save(Stream stream, IAsset asset)
    {
        throw new NotImplementedException();
    }
}