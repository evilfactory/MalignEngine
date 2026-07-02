namespace MalignEngine;

public class HttpAssetSource : IAssetSource
{
    private readonly HttpClient _http;

    public string RootUrl { get; }

    public HttpAssetSource(HttpClient http, string rootUrl)
    {
        _http = http;

        RootUrl = rootUrl.TrimEnd('/') + "/";
    }

    private string GetUrl(string path)
    {
        path = path.Replace('\\', '/').TrimStart('/');

        return RootUrl + path;
    }

    public async Task<Stream> OpenReadAsync(AssetPath path)
    {
        return await _http.GetStreamAsync(GetUrl(path.AbsolutePath));
    }

    public async Task SaveAsync(AssetPath path, Stream stream)
    {
        stream.Position = 0;

        using var content = new StreamContent(stream);

        HttpResponseMessage response =
            await _http.PutAsync(GetUrl(path.AbsolutePath), content);

        response.EnsureSuccessStatusCode();
    }
}