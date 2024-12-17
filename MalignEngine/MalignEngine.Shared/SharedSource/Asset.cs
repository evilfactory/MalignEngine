namespace MalignEngine
{
    public interface IAsset
    {
        public string AssetPath { get; set; }

        public static IAsset Load(string assetPath)
        {
            throw new NotImplementedException();
        }

        public static IAsset CreateDummyAsset()
        {
            throw new NotImplementedException();
        }
    }
}