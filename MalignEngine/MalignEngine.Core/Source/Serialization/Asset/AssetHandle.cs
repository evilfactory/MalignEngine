using System.Reflection;

namespace MalignEngine
{
    public interface IAssetHandle
    {
        public AssetPath AssetPath { get; }
        public bool IsLoading { get; }
        public Task LoadAsync();
        public void Load();
    }

    public class AssetHandle : IAssetHandle
    {
        public bool IsLoading { get; private set; }
        public AssetPath AssetPath { get; private set; }

        private IAsset? asset;
        public IAsset Asset
        {
            get
            {
                if (asset == null)
                {
                    Load();
                }

                if (asset == null)
                {
                    throw new InvalidOperationException("Asset was null after it was loaded.");
                }

                return asset;
            }
            private set
            {
                asset = value;
            }
        }

        public Type AssetType
        {
            get
            {
                if (IsLoading) 
                {
                    if (_loader == null)
                    {
                        throw new InvalidOperationException("Tried to get AssetType while this asset was still loading, but it doesn't have a loaded.");
                    }

                    return _loader.AssetTypes.First(); 
                }

                return Asset.GetType();
            }
        }

        private IAssetLoader? _loader;
        private AssetMount? _mount;

        public AssetHandle(AssetPath assetPath, AssetMount mount, IAssetLoader loader)
        {
            AssetPath = assetPath;
            _loader = loader;
            _mount = mount;
            IsLoading = true;
        }

        public AssetHandle(AssetPath assetPath, IAsset asset)
        {
            IsLoading = false;
            AssetPath = assetPath;
            Asset = asset;
        }

        public void Load()
        {
            LoadAsync().ConfigureAwait(true).GetAwaiter().GetResult();
        }

        public async Task LoadAsync()
        {
            if (_mount == null || _loader == null)
            {
                throw new InvalidOperationException("Tried load asset without a loader.");
            }

            using (var stream = await _mount.Source.OpenReadAsync(AssetPath.RelativeTo(_mount.VirtualRoot)))
            {
                Asset = _loader.Load(stream);
            }

            IsLoading = false;
        }

        /// <summary>
        /// Upgrades the handle to a more specific type
        /// </summary>
        /// <typeparam name="T">Asset Type</typeparam>
        /// <returns>Upgraded handle</returns>
        public AssetHandle<T> Upgrade<T>() where T : class, IAsset
        {
            if (!AssetType.IsAssignableTo(typeof(T)))
            {
                throw new InvalidCastException($"Cannot cast {AssetType} to {typeof(T)}");
            }

            return new AssetHandle<T>(this);
        }

        /// <summary>
        /// Upgrades the handle to a more specific type
        /// </summary>
        /// <typeparam name="T">Asset Type</typeparam>
        /// <returns>Upgraded handle</returns>
        public IAssetHandle Upgrade(Type type)
        {
            if (!AssetType.IsAssignableTo(type))
            {
                throw new InvalidCastException($"Cannot cast {AssetType} to {type}");
            }

            var genericType = typeof(AssetHandle<>).MakeGenericType(type);

            return (IAssetHandle)Activator.CreateInstance(genericType, this)!;
        }
    }

    public class AssetHandle<T> : IAssetHandle where T : class, IAsset
    {
        public AssetPath AssetPath => handle.AssetPath;
        public bool IsLoading => handle.IsLoading;
        public T Asset => (T)handle.Asset;

        private AssetHandle handle;

        public async Task LoadAsync() => await handle.LoadAsync();
        public void Load() => handle.Load();

        public AssetHandle(AssetHandle handle)
        {
            this.handle = handle;
        }

        public static implicit operator T(AssetHandle<T> d) => (T)d.handle.Asset;
    }
}