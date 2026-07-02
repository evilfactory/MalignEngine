namespace MalignEngine;

public sealed class AssetMount
{
    public string VirtualRoot { get; }
    public IAssetSource Source { get; }

    public AssetMount(string virtualRoot, IAssetSource source)
    {
        VirtualRoot = virtualRoot;
        Source = source;
    }
}