namespace MalignEngine;

public class Theme
{
    private readonly Dictionary<string, object> _styles = new();

    public void Add<T>(string key, T style) where T : class
    {
        _styles[key] = style;
    }

    public T Get<T>(string key) where T : class
    {
        return (T)_styles[key];
    }
}