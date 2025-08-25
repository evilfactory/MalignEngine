
namespace MalignEngine;

public class FontAssetLoader : IAssetLoader
{
    public Type GetAssetType(AssetPath assetPath) => typeof(Font); 

    public IEnumerable<string> GetSubIds(AssetPath assetPath)
    {
        return Enumerable.Empty<string>();
    }

    public bool IsCompatible(AssetPath assetPath)
    {
        return assetPath.Extension == "ttf";
    }

    public IAsset Load(AssetPath assetPath)
    {
        AssetSource source = AssetSource.Get(assetPath);

        using (var memoryStream = new MemoryStream())
        {
            source.GetStream().CopyTo(memoryStream);

            return new Font(memoryStream.GetBuffer());
        }
    }

    public void Save(AssetPath assetPath, IAsset asset)
    {
        throw new NotImplementedException();
    }
}