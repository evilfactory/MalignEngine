namespace MalignEngine;

public class FileAssetSource : AssetSource
{
    public FileAssetSource(AssetPath assetPath) : base(assetPath) { }

    private FileStream? openFile;

    public override Stream GetStream()
    {
        if (openFile == null)
        {
            openFile = File.Open(AssetPath.AbsolutePath, FileMode.Open);
        }

        openFile.Position = 0;

        return openFile;
    }

    public override void Dispose()
    {
        openFile?.Dispose();
    }
}