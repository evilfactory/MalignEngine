namespace MalignEngine;

public class FileAssetSource : AssetSource
{
    private static Dictionary<AssetPath, FileStream> openFileMap = new Dictionary<AssetPath, FileStream>();

    public FileAssetSource(AssetPath assetPath) : base(assetPath) { }

    public override Stream GetStream()
    {
        FileStream openFile;

        if (!openFileMap.ContainsKey(AssetPath))
        {
            openFile = File.Open(AssetPath.AbsolutePath, FileMode.Open);
            openFileMap.Add(AssetPath, openFile);
        }
        else
        {
            openFile = openFileMap[AssetPath];
        }

        openFile.Position = 0;

        return openFile;
    }

    public override void Dispose()
    {
        if (openFileMap.TryGetValue(AssetPath, out FileStream? stream))
        {
            stream.Dispose();
            openFileMap.Remove(AssetPath);
        }
    }
}