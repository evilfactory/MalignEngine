namespace MalignEngine;

public interface IAssetLoader
{
    IReadOnlyCollection<Type> AssetTypes { get; }
    IAsset Load(Stream stream);
    void Save(Stream stream, IAsset asset);
}