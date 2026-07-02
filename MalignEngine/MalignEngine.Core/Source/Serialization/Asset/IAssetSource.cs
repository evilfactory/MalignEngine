namespace MalignEngine;

public interface IAssetSource
{
    Task<Stream> OpenReadAsync(AssetPath path);
    Task SaveAsync(AssetPath path, Stream data);
}