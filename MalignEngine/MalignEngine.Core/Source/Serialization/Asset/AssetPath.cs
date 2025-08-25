namespace MalignEngine;

/// <summary>
/// An asset path is used to identify an asset in the asset service, it looks like this: "file:Content/SomeXmlFile.xml#test"
/// The id is only required if the asset type can support multiple assets per file
/// </summary>
public struct AssetPath
{
    public string FullPath { get; private set; }
    public string Source => FullPath.Split(":")[0];
    public string AbsolutePath => FullPath.Split('#')[0].Split(":")[1];
    public string Id => FullPath.Split('#')[1];
    public string Extension => Path.GetExtension(AbsolutePath).Substring(1);

    public AssetPath(string path)
    {
        FullPath = path;
    }

    public static implicit operator AssetPath(string path) => new AssetPath(path);
    public static implicit operator string(AssetPath path) => path.FullPath;
}