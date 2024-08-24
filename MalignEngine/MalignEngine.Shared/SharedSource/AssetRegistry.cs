using System.Collections.Concurrent;

namespace MalignEngine
{
    public class AssetSystem : BaseSystem
    {
        private Dictionary<string, Asset> assets;

        public AssetSystem()
        {
            assets = new Dictionary<string, Asset>();
        }

        public void Add<T>(T asset) where T : Asset
        {
            if (assets.ContainsKey(asset.Identifier))
            {
                throw new ArgumentException($"Asset with identifier {asset.Identifier} already exists.");
            }

            assets.Add(asset.Identifier, asset);
        }

        public T Get<T>(string identifier) where T : Asset
        {
            if (!assets.ContainsKey(identifier))
            {
                throw new ArgumentException($"Asset with identifier {identifier} does not exist.");
            }

            return (T)assets[identifier];
        }

        public IEnumerable<Asset> GetAll()
        {
            return assets.Values;
        }

        public IEnumerable<Asset> GetOfType<T>() where T : Asset
        {
            return assets.Values.Where(asset => asset is T);
        }
    }
}