using System.Reflection;

namespace MalignEngine
{
    public interface IAssetHandle
    {
        public bool IsLoading { get; }
        public void LoadNow();
    }
    public class AssetHandle<T> : IAssetHandle where T : IAsset
    {
        public static T DummyAsset;

        public bool IsLoading { get; private set; }
        public string AssetPath { get; private set; }
        private T asset;
        public T Asset
        {
            get
            {
                if (asset == null && DummyAsset != null)
                {
                    return DummyAsset;
                }
                else if (asset == null)
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

        public AssetHandle(string assetPath)
        {
            IsLoading = true;
            AssetPath = assetPath;

            if (DummyAsset == null)
            {
                MethodInfo? method = typeof(T).GetMethod("CreateDummyAsset", BindingFlags.Public | BindingFlags.Static);
                IAsset? result = (IAsset?)method?.Invoke(null, new object[] { });

                if (result == null)
                {
                    //throw new Exception($"Dummy asset returned was null");
                }

                DummyAsset = (T)result;
            }
        }

        public AssetHandle(string assetPath, T asset)
        {
            IsLoading = false;
            AssetPath = assetPath;
            Asset = asset;
        }

        public void LoadNow()
        {
            Type type = typeof(T);

            object? result;

            try
            {
                result = type.GetMethod("Load", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { AssetPath });
            }
            catch
            {
                throw;
            }

            if (result == null)
            {
                throw new Exception($"Asset returned was null");
            }

            Asset = (T)result;
            IsLoading = false;
        }

        public static implicit operator T(AssetHandle<T> d) => d.Asset;
    }
}