namespace MalignEngine;

public interface IAssetLoader
{
    Type AssetType { get; }
    bool IsCompatible(AssetPath assetPath);
    IEnumerable<string> GetSubIds(AssetPath assetPath);
    IAsset Load(AssetPath assetSource);
}