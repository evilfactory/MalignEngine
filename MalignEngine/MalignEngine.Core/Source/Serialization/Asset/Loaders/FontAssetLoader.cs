
namespace MalignEngine;

public class FontAssetLoader : IAssetLoader
{
    public Type AssetType => typeof(Font);

    public FontAssetLoader()
    { 
    }

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
        Font font = new Font();
        font.LoadFromPath(assetPath);
        return font;
    }
}