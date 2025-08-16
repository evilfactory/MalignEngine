using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine;

public interface IAssetService
{
    public AssetHandle FromPath(AssetPath assetPath);
    public AssetHandle<T> FromPath<T>(AssetPath assetPath) where T : class, IAsset;
}

public class AssetService : IService, IUpdate
{
    private ILogger _logger;

    private Dictionary<AssetPath, AssetHandle> _assetHandles;
    private Queue<IAssetHandle> _loadingQueue;
    private IServiceContainer _container;

    public AssetService(ILoggerService loggerService, IServiceContainer container)
    {
        _logger = loggerService.GetSawmill("assets");

        _assetHandles = new Dictionary<AssetPath, AssetHandle>();
        _loadingQueue = new Queue<IAssetHandle>();
        _container = container;
    }


    public void OnUpdate(float deltatime)
    {
        if (_loadingQueue != null)
        {
            int assetsLoaded = 0;
            while (_loadingQueue.Count > 0)
            {
                IAssetHandle handle = _loadingQueue.Dequeue();

                if (handle.IsLoading)
                {
                    handle.LoadNow();
                }

                assetsLoaded++;

                if (assetsLoaded > 10)
                {
                    break;
                }
            }
        }
    }

    public void LoadFolder(string folderPath)
    {
        throw new NotImplementedException();
    }

    public AssetHandle FromPath(AssetPath assetPath)
    {
        if (_assetHandles.ContainsKey(assetPath))
        {
            return _assetHandles[assetPath];
        }

        IEnumerable<IAssetLoader> loaders = _container.GetInstances<IAssetLoader>();

        var loader = loaders.Where(x => x.IsCompatible(assetPath)).First();

        AssetHandle handle = new AssetHandle(assetPath, loader);

        return handle;
    }

    public AssetHandle<T> FromPath<T>(AssetPath assetPath) where T : class, IAsset
    {
        return FromPath(assetPath).Upgrade<T>();
    }
}