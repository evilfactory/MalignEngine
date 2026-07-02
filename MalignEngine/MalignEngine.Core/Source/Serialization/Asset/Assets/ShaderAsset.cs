namespace MalignEngine;

public class ShaderAsset : IAsset
{
    public IShaderResource ShaderResource { get; private set; }

    public ShaderAsset(IShaderResource shaderResource)
    {
        ShaderResource = shaderResource;
    }
}
