
namespace MalignEngine;

public class FontAssetLoader : IAssetLoader
{
    public IReadOnlyCollection<Type> AssetTypes => [typeof(Font)];

    public IAsset Load(Stream stream)
    {
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);

            return new Font(memoryStream.GetBuffer());
        }
    }

    public void Save(Stream stream, IAsset asset)
    {
        throw new NotImplementedException();
    }
}