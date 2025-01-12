using System.Reflection;

namespace MalignEngine
{
    public interface IAssetHandle
    {
        public AssetPath AssetPath { get; }
        public bool IsLoading { get; }
        public void LoadNow();
    }

    public class AssetHandle : IAssetHandle
    {
        public bool IsLoading { get; private set; }
        public AssetPath AssetPath { get; private set; }
        private IAsset asset;
        public IAsset Asset
        {
            get
            {
                if (asset == null)
                {
                    LoadNow();
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
                if (IsLoading) { return loader.AssetType; }

                return Asset.GetType();
            }
        }

        private IAssetFileLoader loader;

        public AssetHandle(AssetPath assetPath, IAssetFileLoader loader)
        {
            IsLoading = true;
            AssetPath = assetPath;
            this.loader = loader;
        }

        public AssetHandle(AssetPath assetPath, IAsset asset)
        {
            IsLoading = false;
            AssetPath = assetPath;
            Asset = asset;
        }

        public void LoadNow()
        {
            Asset = loader.Load();

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
    }

    public class AssetHandle<T> : IAssetHandle where T : class, IAsset
    {
        public AssetPath AssetPath => handle.AssetPath;
        public bool IsLoading => handle.IsLoading;
        public T Asset => (T)handle.Asset;

        private AssetHandle handle;

        public void LoadNow() => handle.LoadNow();

        public AssetHandle(AssetHandle handle)
        {
            this.handle = handle;
        }

        public static implicit operator T(AssetHandle<T> d) => (T)d.handle.Asset;
    }
}