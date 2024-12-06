namespace MalignEngine
{
    public class Material : IAsset
    {
        public Shader Shader { get; set; }
        public bool UseTextureBatching { get; set; } = true;

        private Dictionary<string, object> properties = new Dictionary<string, object>();

        public Material(Shader shader)
        {
            Shader = shader;
        }

        public IEnumerable<KeyValuePair<string, object>> GetProperties()
        {
            return properties;
        }

        public void SetProperty(string name, object value)
        {
            properties[name] = value;
        }

        public void GetProperty(string name, out object value)
        {
            properties.TryGetValue(name, out value);
        }
    }
}