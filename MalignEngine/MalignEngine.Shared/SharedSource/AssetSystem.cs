using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;

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

        public void LoadFolder(string folderPath)
        {
            // Find all files and files in subdirectories
            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string ext = Path.GetExtension(file);

                switch (ext)
                {
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                        Load<Texture2D>(file);
                        break;
                    case ".xml":
                        // Figure out what this is first (check root element)
                        XElement xml = XElement.Load(file);
                        string rootElement = xml.Name.LocalName;

                        if (rootElement == "Scene")
                        {
                            Load<Scene>(file);
                        }
                        break;
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

        public AssetHandle<T> Add<T>(T asset) where T : IAsset => Add(asset, Guid.NewGuid().ToString());

        public AssetHandle<T> Add<T>(T asset, string assetPath) where T : IAsset
        {
            AssetHandle<T> handle = new AssetHandle<T>(assetPath, asset);
            assetHandles.Add(assetPath, handle);

            return handle;
        }

        public List<AssetHandle<T>> GetOfType<T>() where T : IAsset
        {
            List<AssetHandle<T>> handles = new List<AssetHandle<T>>();

            foreach (KeyValuePair<string, IAssetHandle> kvp in assetHandles)
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