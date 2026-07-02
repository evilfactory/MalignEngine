namespace MalignEngine;

public class FileAssetSource : IAssetSource
{
    public string RootDirectory { get; }

    public FileAssetSource(string rootDirectory)
    {
        RootDirectory = Path.GetFullPath(rootDirectory);
    }

    private string GetFullPath(string path)
    {
        path = path.Replace('/', Path.DirectorySeparatorChar);

        return Path.Combine(RootDirectory, path);
    }

    public Task<Stream> OpenReadAsync(AssetPath path)
    {
        Stream stream = File.OpenRead(GetFullPath(path.AbsolutePath));
        return Task.FromResult(stream);
    }

    public async Task SaveAsync(AssetPath path, Stream stream)
    {
        string fullPath = GetFullPath(path.AbsolutePath);

        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using FileStream file = File.Create(fullPath);
        stream.Position = 0;
        await stream.CopyToAsync(file);
    }
}