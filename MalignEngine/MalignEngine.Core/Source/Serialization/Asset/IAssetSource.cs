namespace MalignEngine;

public abstract class AssetSource : IDisposable
{
    public static AssetSource Get(AssetPath path)
    {
        switch (path.Source)
        {
            case "file":
                return new FileAssetSource(path);
            default:
                throw new Exception("Invalid source");
        }
    }

    public AssetPath AssetPath { get; private set; }

    public AssetSource(AssetPath assetPath)
    {
        AssetPath = assetPath;
    }

    public abstract Stream GetStream();

    public abstract void Dispose();
}

