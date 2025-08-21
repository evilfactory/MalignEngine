using System.Runtime.InteropServices;
using System.Numerics;

namespace MalignEngine.Experimentation;

class Experimentation : IService, IDraw
{
    private IRenderingAPI _renderAPI;
    private IRenderer2D _render2D;
    private IWindowService _windowService;

    private IShaderResource _shaderResource;
    private IShaderResource _shaderResource2;
    private ITextureResource _textureResource;
    private IBufferResource _bufferResource;
    private IVertexArrayResource _vertexArrayResource;
    private IFrameBufferResource _frameBufferResource;

    public Experimentation(IRenderingAPI renderAPI, IRenderer2D render2D, IWindowService windowService)
    {
        _renderAPI = renderAPI;
        _render2D = render2D;
        _windowService = windowService;

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

        _textureResource = _renderAPI.CreateTexture(TextureLoader.Load("Content/Textures/player.png"));

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

        _bufferResource = _renderAPI.CreateBuffer(new BufferResourceDescriptor(BufferObjectType.Vertex, BufferUsageType.Static, MemoryMarshal.AsBytes(imageData.AsSpan()).ToArray()));
        
        _frameBufferResource = _renderAPI.CreateFrameBuffer(new FrameBufferDescriptor(1, 1280, 800));
    }

    public void OnDraw(float deltaTime)
    {
        var matrix = Matrix4x4.CreateOrthographicOffCenter(0, _windowService.FrameSize.X, 0, _windowService.FrameSize.Y, 0.0001f, 100f);

        _renderAPI.Submit((IRenderContext ctx) =>
        {
            ctx.SetFrameBuffer(null, _windowService.FrameSize.X, _windowService.FrameSize.Y);
            ctx.Clear(Color.Red);

            ctx.SetBlendingMode(BlendingMode.AlphaBlend);
            _render2D.SetMatrix(matrix);
            _render2D.Begin(ctx);

            Vector2 scale = new Vector2(_windowService.FrameSize.X / 32f, _windowService.FrameSize.X / 32f);

            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    _render2D.DrawTexture2D(_textureResource, new Vector2(x * scale.X, y * scale.Y), new Vector2(scale.X, scale.Y), 0f);
                }
            }
            _render2D.End();

            /*
            ctx.SetShader(_shaderResource);
            ctx.SetTexture(0, _textureResource);
            ctx.DrawArrays(_bufferResource, _vertexArrayResource, 6);

            ctx.SetFrameBuffer(null, 1280, 800);
            ctx.Clear(Color.Green);
            ctx.SetShader(_shaderResource2);
            ctx.SetTexture(0, _frameBufferResource.GetColorAttachment(0));
            ctx.DrawArrays(_bufferResource, _vertexArrayResource, 6);
            */
        });

    }
}
