using System.Runtime.InteropServices;

namespace MalignEngine.Experimentation;

class Experimentation : IService, IDraw
{
    private IRenderingAPI _renderAPI;
    private IShaderResource _shaderResource;
    private IShaderResource _shaderResource2;
    private ITextureResource _textureResource;
    private IBufferResource _bufferResource;
    private IVertexArrayResource _vertexArrayResource;
    private IFrameBufferResource _frameBufferResource;

    public Experimentation(IRenderingAPI renderAPI)
    {
        _renderAPI = renderAPI;

        _shaderResource = _renderAPI.CreateShader(new ShaderResourceDescriptor()
        {
            FragmentShaderSource = File.ReadAllText("Content/TestFrag.glsl"),
            VertexShaderSource = File.ReadAllText("Content/TestVert.glsl")
        });

        _shaderResource2 = _renderAPI.CreateShader(new ShaderResourceDescriptor()
        {
            FragmentShaderSource = File.ReadAllText("Content/TestFrag2.glsl"),
            VertexShaderSource = File.ReadAllText("Content/TestVert.glsl")
        });

        _textureResource = _renderAPI.CreateTexture(TextureLoader.Load("Content/Textures/light.png"));

        var desc = new VertexArrayDescriptor();
        desc.AddAttribute("Color", 0, VertexAttributeType.Float, 3, false);
        desc.AddAttribute("UV", 1, VertexAttributeType.Float, 2, false);
        _vertexArrayResource = _renderAPI.CreateVertexArray(desc);

        float[] imageData = new float[]
        {
            -1, -1, 0f,     0f, 0f, // Bottom-left
             1, -1, 0f,     1f, 0f, // Bottom-right
             1,  1, 0f,     1f, 1f, // Top-right

            // Triangle 2
            -1, -1, 0f,     0f, 0f, // Bottom-left
             1,  1, 0f,     1f, 1f, // Top-right
            -1,  1, 0f,     0f, 1f  // Top-left
        };

        ReadOnlySpan<float> floatSpan = imageData;
        ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(floatSpan);
        byte[] bytes = byteSpan.ToArray();

        _bufferResource = _renderAPI.CreateBuffer(new BufferResourceDescriptor(BufferObjectType.Vertex, BufferUsageType.Static, bytes));

        _frameBufferResource = _renderAPI.CreateFrameBuffer(new FrameBufferDescriptor(1, 1280, 800));
    }

    public void OnDraw(float deltaTime)
    {
        _renderAPI.Submit((IRenderContext ctx) =>
        {
            ctx.SetFrameBuffer(_frameBufferResource);
            ctx.Clear(Color.Red);
            ctx.SetShader(_shaderResource);
            ctx.SetTexture(0, _textureResource);
            ctx.DrawArrays(_bufferResource, _vertexArrayResource, 6);

            ctx.SetFrameBuffer(null, 1280, 800);
            ctx.Clear(Color.Green);
            ctx.SetShader(_shaderResource2);
            ctx.SetTexture(0, _frameBufferResource.GetColorAttachment(0));
            ctx.DrawArrays(_bufferResource, _vertexArrayResource, 6);
        });

    }
}
