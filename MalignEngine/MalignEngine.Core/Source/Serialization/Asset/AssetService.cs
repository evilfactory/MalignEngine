using nkast.Aether.Physics2D.Common;

namespace MalignEngine;

public interface IAssetService
{
    void Mount(AssetPath virtualRoot, IAssetSource source);
    /// <summary>
    /// Returns all handles associated with a specific asset path, without a specific id
    /// </summary>
    //IEnumerable<AssetHandle> FromPathAll(AssetPath assetPath);
    AssetHandle FromPath(Type type, AssetPath assetPath);
    /// <summary>
    /// Gets the handle for the specified asset path. Throws an exception if there's more than one handle in this asset path and there's no id specified in the asset path
    /// </summary>
    AssetHandle<T> FromPath<T>(AssetPath assetPath) where T : class, IAsset;
    AssetHandle<T> FromAsset<T>(T asset) where T : class, IAsset;
    IEnumerable<AssetHandle<T>> GetHandles<T>() where T : class, IAsset;
    //void Save(AssetPath assetPath, IAsset asset);
    Task PreLoadAsync(AssetManifest manifest);
}

public class AssetService : IAssetService, IService
{
    private ILogger _logger;

    private Dictionary<(Type Type, AssetPath Path), AssetHandle> _assetHandles;
    private List<AssetMount> _mounts;

    private IServiceContainer _container;

    public AssetService(ILoggerService loggerService, IServiceContainer container)
    {
        _logger = loggerService.GetSawmill("assets");

        _mounts = new List<AssetMount>();
        _assetHandles = new Dictionary<(Type Type, AssetPath Path), AssetHandle>();
        _container = container;
    }

    public void Mount(AssetPath virtualRoot, IAssetSource source)
    {
        _mounts.Add(new AssetMount(virtualRoot, source));
    }

    public AssetHandle<T> FromPath<T>(AssetPath assetPath) where T : class, IAsset => FromPath(typeof(T), assetPath).Upgrade<T>();

    public AssetHandle FromPath(Type type, AssetPath assetPath)
    {
        if (_assetHandles.TryGetValue((type, assetPath), out AssetHandle? handle))
        {
            return handle;
        }

        IEnumerable<IAssetLoader> loaders = _container.GetInstance<IEnumerable<IAssetLoader>>();

        IAssetLoader? loader = loaders.FirstOrDefault(l => l.AssetTypes.Contains(type));
        
        if (loader == null)
        {
            throw new ArgumentException($"Failed to find loader for type {type.Name}");
        }

        AssetMount? mount = FindMount(assetPath);

        if (mount == null)
        {
            throw new ArgumentException($"Failed to find mount that matches path {assetPath}");
        }

        handle = new AssetHandle(assetPath, mount, loader);
        _assetHandles[(type, assetPath)] = handle;

        return handle;
    }

    public AssetHandle<T> FromAsset<T>(T asset) where T : class, IAsset
    {
        AssetPath assetPath = new AssetPath("none:" + Guid.NewGuid().ToString("N"));

        var handle = new AssetHandle(assetPath, asset);

        _assetHandles[(asset.GetType(), assetPath)] = handle;

        return handle.Upgrade<T>();
    }

    IEnumerable<AssetHandle<T>> IAssetService.GetHandles<T>()
    {
        foreach (var handle in _assetHandles.Values)
        {
            if (handle.AssetType == typeof(T))
            {
                yield return handle.Upgrade<T>();
            }
        }
    }

    private AssetMount? FindMount(AssetPath assetPath)
    {
        AssetMount? best = null;

        foreach (var mount in _mounts)
        {
            if (!assetPath.ToString().StartsWith(mount.VirtualRoot))
            {
                continue;
            }

            if (best == null ||
                mount.VirtualRoot.Length > best.VirtualRoot.Length)
            {
                best = mount;
            }
        }

        return best;
    }

    public async Task PreLoadAsync(AssetManifest manifest)
    {
        foreach ((Type type, AssetPath path) in manifest.Assets)
        {
            AssetHandle handle = FromPath(type, path);
            await handle.LoadAsync();
        }
    }
}