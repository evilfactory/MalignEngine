
namespace MalignEngine;

public class TextureAssetLoader : IService, IAssetLoader
{
    private IRenderingAPI _renderingAPI;

    public TextureAssetLoader(IRenderingAPI renderingAPI)
    {
        _renderingAPI = renderingAPI;
    }

    public Type GetAssetType(AssetPath assetPath)
    {
        return typeof(Texture2D);
    }

    public bool IsCompatible(AssetPath assetPath)
    {
        return assetPath.Extension == "png" || assetPath.Extension == "jpg" || assetPath.Extension == "jpeg";
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

    public void Save(AssetPath assetPath, IAsset asset)
    {
        throw new NotImplementedException();
    }
}