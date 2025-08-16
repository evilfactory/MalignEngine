
namespace MalignEngine;

public class FontAssetLoader : IAssetLoader
{
    public Type AssetType => typeof(Font);

    public FontAssetLoader()
    { 
    }

    public bool IsCompatible(AssetPath assetPath)
    {
        return assetPath.Extension == "ttf";
    }

    public IEnumerable<IAsset> Load(AssetPath assetPath)
    {
        Font font = new Font();
        font.LoadFromPath(assetPath);
        return new List<IAsset>() { (IAsset)font };
    }
}