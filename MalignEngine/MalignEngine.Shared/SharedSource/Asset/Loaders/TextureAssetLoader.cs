namespace MalignEngine;

public class TextureAssetLoaderFactory : IAssetFileLoaderFactory
{
    public bool CanLoadExtension(string extension)
    {
        return extension == ".png" || extension == ".jpg" || extension == ".jpeg";
    }

    public IAssetFileLoader[] CreateLoaders(string file)
    {
        return new IAssetFileLoader[] { new TextureAssetLoader(new AssetPath(file)) };
    }
}

public class TextureAssetLoader : AssetFileLoader
{
    public TextureAssetLoader(AssetPath assetPath) : base(assetPath) { }

    public override IAsset Load()
    {
        Texture2D texture = new Texture2D();
        texture.LoadFromPath(AssetPath.PathWithoutId);
        return texture;
    }
}