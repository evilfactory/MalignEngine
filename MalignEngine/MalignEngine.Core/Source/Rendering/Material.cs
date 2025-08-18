namespace MalignEngine;

public interface IMaterial
{
    IShaderResource Shader { get; }
    void SetProperty(string name, object value);
    object? GetProperty(string name);
    IEnumerable<KeyValuePair<string, object>> Properties { get; }
}

public class Material : IMaterial
{
    public IShaderResource Shader { get; private set; }
    public IEnumerable<KeyValuePair<string, object>> Properties => _properties;

    private Dictionary<string, object> _properties = new Dictionary<string, object>();

    public Material(IShaderResource shader)
    {
        Shader = shader;
    }

    public void SetProperty(string name, object value)
    {
        _properties[name] = value;
    }

    public object? GetProperty(string name)
    {
        if (_properties.TryGetValue(name, out var value))
        {
            return value;
        }

        return null;
    }
}