
namespace MalignEngine;

public class TextureAssetLoader : IService, IAssetLoader
{
    public Type AssetType => typeof(Texture2D);

    private IRenderingAPI _renderingAPI;

    public TextureAssetLoader(IRenderingAPI renderingAPI)
    {
        _renderingAPI = renderingAPI;
    }

    public bool IsCompatible(AssetPath assetPath)
    {
        return assetPath.Extension == "png";
    }

    public IEnumerable<string> GetSubIds(AssetPath assetPath)
    {
        return Enumerable.Empty<string>();
    }

    public IAsset Load(AssetPath assetPath)
    {
        AssetSource source = AssetSource.Get(assetPath);

        ITextureDescriptor descriptor = TextureLoader.Load(source.GetStream());
        ITextureResource resource = _renderingAPI.CreateTexture(descriptor);
        Texture2D texture = new Texture2D(resource);
        return texture;
    }
}