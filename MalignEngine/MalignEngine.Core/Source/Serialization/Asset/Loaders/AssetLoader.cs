namespace MalignEngine;

public interface IAssetLoader
{
    Type GetAssetType(AssetPath path);
    bool IsCompatible(AssetPath assetPath);
    IEnumerable<string> GetSubIds(AssetPath assetPath);
    IAsset Load(AssetPath assetPath);
}