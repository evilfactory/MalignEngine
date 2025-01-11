namespace MalignEngine;

public class FontAssetLoaderFactory : IAssetFileLoaderFactory
{
    public bool CanLoadExtension(string extension)
    {
        return extension == ".ttf";
    }

    public IAssetFileLoader[] CreateLoaders(string file)
    {
        return new IAssetFileLoader[] { new FontAssetLoader(new AssetPath(file)) };
    }
}

public class FontAssetLoader : AssetFileLoader
{
    public FontAssetLoader(AssetPath assetPath) : base(assetPath) { }

    public override IAsset Load()
    {
        Font font = new Font();
        font.LoadFromPath(AssetPath);
        return font;
    }
}