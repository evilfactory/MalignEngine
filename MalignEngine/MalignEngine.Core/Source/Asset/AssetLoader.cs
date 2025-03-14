namespace MalignEngine;

public interface IAssetFileLoaderFactory
{
    public bool CanLoadExtension(string extension);
    public IAssetFileLoader[] CreateLoaders(string file);
}

public interface IAssetFileLoader
{
    public AssetPath AssetPath { get; }
    public Type AssetType { get; }
    public IAsset Load();
}

public abstract class AssetFileLoader : IAssetFileLoader
{
    public AssetPath AssetPath { get; private set; }
    public abstract Type AssetType { get; }

    public AssetFileLoader(AssetPath assetPath)
    {
        AssetPath = assetPath;
    }

    public abstract IAsset Load();
}