using System.Collections.Concurrent;
using System.Reflection;

namespace MalignEngine
{
    public class AssetSystem : BaseSystem
    {
        private Dictionary<string, IAssetHandle> assetHandles;

        private Queue<IAssetHandle> loadingQueue;

        public AssetSystem()
        {
            assetHandles = new Dictionary<string, IAssetHandle>();
            loadingQueue = new Queue<IAssetHandle>();
        }

        public override void OnInitialize()
        {

        }

        public override void OnUpdate(float deltatime)
        {
            if (loadingQueue != null)
            {
                int assetsLoaded = 0;
                while (loadingQueue.Count > 0)
                {
                    IAssetHandle handle = loadingQueue.Dequeue();

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

        public AssetHandle<T> Load<T>(string assetPath, bool lazyLoad = false) where T : IAsset
        {
            if (assetHandles.ContainsKey(assetPath))
            {
                return (AssetHandle<T>)assetHandles[assetPath];
            }

            AssetHandle<T> handle = new AssetHandle<T>(assetPath);

            assetHandles.Add(assetPath, handle);

            if (lazyLoad)
            {
                loadingQueue.Enqueue(handle);
            }
            else
            {
                handle.LoadNow();
            }

            return handle;
        }

        public AssetHandle<T> Add<T>(T asset) where T : IAsset
        {
            string assetPath = Guid.NewGuid().ToString();

            AssetHandle<T> handle = new AssetHandle<T>(assetPath, asset);
            assetHandles.Add(assetPath, handle);

            return handle;
        }
    }
}