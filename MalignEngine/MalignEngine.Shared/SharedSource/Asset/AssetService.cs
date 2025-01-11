using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;

namespace MalignEngine
{
    /// <summary>
    /// An asset path is used to identify an asset in the asset service, it looks like this: "Content/SomeXmlFile.xml#Identifier"
    /// </summary>
    public struct AssetPath
    {
        public string FullPath { get; private set; }
        public string PathWithoutId => FullPath.Split('#')[0];
        public string Id => FullPath.Split('#')[1];
        public string Extension => Path.GetExtension(PathWithoutId);

        public AssetPath(string path)
        {
            FullPath = path;
        }

        public static implicit operator AssetPath(string path) => new AssetPath(path);
        public static implicit operator string(AssetPath path) => path.FullPath;
    }

    public class AssetService : IService, IInit, IUpdate
    {
        [Dependency]
        protected LoggerService LoggerService = default!;
        protected ILogger logger;

        private Dictionary<AssetPath, AssetHandle> assetHandles;

        private List<IAssetFileLoaderFactory> assetLoaders;

        private Queue<IAssetHandle> loadingQueue;

        public AssetService()
        {
            assetHandles = new Dictionary<AssetPath, AssetHandle>();
            assetLoaders = new List<IAssetFileLoaderFactory>();
            loadingQueue = new Queue<IAssetHandle>();
        }

        public void OnInitialize()
        {
            logger = LoggerService.GetSawmill("assets");

            RegisterLoader(new TextureAssetLoaderFactory());
            RegisterLoader(new FontAssetLoaderFactory());
            RegisterLoader(new XmlAssetLoaderFactory());
        }

        public void OnUpdate(float deltatime)
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

        public void RegisterLoader(IAssetFileLoaderFactory loader)
        {
            assetLoaders.Add(loader);
        }

        public void LoadFolder(string folderPath)
        {
            // Find all files and files in subdirectories
            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                IAssetFileLoaderFactory? loader = assetLoaders.FirstOrDefault(l => l.CanLoadExtension(Path.GetExtension(file)));

                if (loader != null)
                {
                    FromFile(file);
                }
            }
        }

        public AssetHandle FromFile(AssetPath assetPath)
        {
            if (assetHandles.ContainsKey(assetPath))
            {
                return assetHandles[assetPath];
            }

            IAssetFileLoaderFactory? loader = assetLoaders.FirstOrDefault(l => l.CanLoadExtension(assetPath.Extension));

            if (loader == null)
            {
                throw new Exception($"No loader found for {assetPath}");
            }

            IAssetFileLoader[] loaders = loader.CreateLoaders(assetPath.PathWithoutId);

            AssetHandle[] handles = loaders.Select(loader => new AssetHandle(assetPath, loader)).ToArray();

            foreach (AssetHandle handle in handles)
            {
                assetHandles.Add(handle.AssetPath, handle);
                loadingQueue.Enqueue(handle);
            }

            return handles.Where(h => h.AssetPath == assetPath).First();
        }

        public AssetHandle<T> FromFile<T>(AssetPath assetPath) where T : class, IFileLoadableAsset<T>, new() => FromFile(assetPath).Upgrade<T>();

        public AssetHandle<T> FromAsset<T>(T asset) where T : class, IFileLoadableAsset<T>, new()
        {
            string assetPath = Guid.NewGuid().ToString();
            AssetHandle handle = new AssetHandle(assetPath, asset);

            assetHandles.Add(assetPath, handle);

            return handle.Upgrade<T>();
        }

        public AssetHandle<T> GetFromId<T>(string identifier) where T : class, IFileLoadableAsset<T>, IAssetWithId, new()
        {
            // Go through all assets that implement IAssetWithId and check if the id matches
            foreach (KeyValuePair<AssetPath, AssetHandle> kvp in assetHandles)
            {
                if (kvp.Value is AssetHandle<T> handle)
                {
                    if (handle.Asset.AssetId == identifier)
                    {
                        return handle;
                    }
                }
            }

            return null;
        }

        public List<AssetHandle<T>> GetOfType<T>() where T : class, IFileLoadableAsset<T>, new()
        {
            List<AssetHandle<T>> handles = new List<AssetHandle<T>>();

            foreach (KeyValuePair<AssetPath, AssetHandle> kvp in assetHandles)
            {
                if (kvp.Value is AssetHandle<T> handle)
                {
                    handles.Add(handle);
                }
            }

            return handles;
        }
    }
}