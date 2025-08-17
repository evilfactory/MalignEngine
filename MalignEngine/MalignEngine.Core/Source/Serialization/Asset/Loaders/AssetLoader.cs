namespace MalignEngine;

public interface IAssetLoader
{
    Type AssetType { get; }
    bool IsCompatible(AssetPath assetPath);
    IEnumerable<IAsset> Load(AssetPath assetSource);
}