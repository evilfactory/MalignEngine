namespace MalignEngine;

public interface IAssetService
{
    /// <summary>
    /// Returns all handles associated with a specific asset path, without a specific id
    /// </summary>
    IEnumerable<AssetHandle> FromPathAll(AssetPath assetPath);
    /// <summary>
    /// Gets the handle for the specified asset path. Throws an exception if there's more than one handle in this asset path and there's no id specified in the asset path
    /// </summary>
    AssetHandle FromPath(AssetPath assetPath);
    /// <inheritdoc cref="FromPath" />
    AssetHandle<T> FromPath<T>(AssetPath assetPath) where T : class, IAsset;
    void Save(AssetPath assetPath, IAsset asset);
}

public class AssetService : IAssetService, IService
{
    private ILogger _logger;

    private Dictionary<AssetPath, AssetHandle> _assetHandles;
    private IServiceContainer _container;

    public AssetService(ILoggerService loggerService, IServiceContainer container)
    {
        _logger = loggerService.GetSawmill("assets");

        _assetHandles = new Dictionary<AssetPath, AssetHandle>();
        _container = container;
    }

    public IEnumerable<AssetHandle> FromPathAll(AssetPath assetPath)
    {
        IEnumerable<IAssetLoader> loaders = _container.GetInstance<IEnumerable<IAssetLoader>>();

        var loader = loaders.Where(x => x.IsCompatible(assetPath)).FirstOrDefault();

        if (loader == null)
        {
            throw new InvalidOperationException("No loader defined for this asset extension");
        }

        List<AssetHandle> handles = new List<AssetHandle>();

        foreach (var id in loader.GetSubIds(assetPath))
        {
            AssetPath assetPathWithId = assetPath + "#" + id;

            handles.Add(FromPath(assetPathWithId));
        }

        return handles;
    }

    public AssetHandle FromPath(AssetPath assetPath)
    {
        if (_assetHandles.ContainsKey(assetPath))
        {
            return _assetHandles[assetPath];
        }

        IEnumerable<IAssetLoader> loaders = _container.GetInstance<IEnumerable<IAssetLoader>>();

        var loader = loaders.Where(x => x.IsCompatible(assetPath)).FirstOrDefault();

        if (loader == null)
        {
            throw new InvalidOperationException("No loader defined for this asset extension");
        }

        IEnumerable<string> subIds = loader.GetSubIds(assetPath);

        bool matchesAny = false;

        if (subIds.Count() == 0)
        {
            matchesAny = true;
        }
        else
        {
            foreach (var str in subIds)
            {
                if (str == assetPath.Id)
                {
                    matchesAny = true;
                }
            }
        }

        if (!matchesAny)
        {
            throw new InvalidOperationException("Asset id is invalid");
        }

        AssetHandle handle = new AssetHandle(assetPath, loader);

        _assetHandles[assetPath] = handle;

        return handle;
    }

    public AssetHandle<T> FromPath<T>(AssetPath assetPath) where T : class, IAsset
    {
        return FromPath(assetPath).Upgrade<T>();
    }

    public void Save(AssetPath assetPath, IAsset asset)
    {
        IEnumerable<IAssetLoader> loaders = _container.GetInstance<IEnumerable<IAssetLoader>>();

        var loader = loaders.Where(x => x.IsCompatible(assetPath)).FirstOrDefault();

        if (loader == null)
        {
            throw new InvalidOperationException("No loader defined for this asset extension");
        }

        throw new NotImplementedException();
    }
}