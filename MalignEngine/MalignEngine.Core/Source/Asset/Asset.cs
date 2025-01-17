
namespace MalignEngine;

public interface IAsset { }

public interface IFileLoadableAsset<T> : IAsset where T : class, new()
{
    public T LoadFromPath(AssetPath assetPath);
}

/*
 * public interface ILazyLoadableAsset<T> where T : class
{
    public abstract static T CreateDummyAsset();
}
*/

public interface IAssetWithId
{
    public string AssetId { get; }
}