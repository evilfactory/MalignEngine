using System.Runtime.InteropServices;
using System.Numerics;

namespace MalignEngine.Samples.Cubes;

class Cubes : IService, IDraw
{
    private IRenderingAPI _renderAPI;
    private IWindowService _windowService;
    private IShaderResource _shaderResource;
    private ITextureResource _textureResource;
    private IBufferResource _bufferResource;
    private IVertexArrayResource _vertexArrayResource;
    private IPipelineResource _pipelineResource;

    private Matrix4x4[] _cubeTransforms;

    public Cubes(IRenderingAPI renderAPI, IWindowService windowService)
    {
        _renderAPI = renderAPI;
        _windowService = windowService;

        _shaderResource = _renderAPI.CreateShader(new ShaderResourceDescriptor()
        {
            VertexShaderSource = File.ReadAllText("Content/TestVert.glsl"),
            FragmentShaderSource = File.ReadAllText("Content/TestFrag.glsl")
        });

        _textureResource = _renderAPI.CreateTexture(TextureLoader.Load("Content/Textures/wolfgang.jpg"));

        var desc = new VertexArrayDescriptor();
        desc.AddAttribute("Position", 0, VertexAttributeType.Float, 3, false);
        desc.AddAttribute("UV", 1, VertexAttributeType.Float, 2, false);
        _vertexArrayResource = _renderAPI.CreateVertexArray(desc);

        float[] vertices = new float[]
        {
            // Front face
            -0.5f, -0.5f,  0.5f, 0,0,
             0.5f, -0.5f,  0.5f, 1,0,
             0.5f,  0.5f,  0.5f, 1,1,
            -0.5f, -0.5f,  0.5f, 0,0,
             0.5f,  0.5f,  0.5f, 1,1,
            -0.5f,  0.5f,  0.5f, 0,1,

            // Back face
            -0.5f, -0.5f, -0.5f, 0,0,
             0.5f,  0.5f, -0.5f, 1,1,
             0.5f, -0.5f, -0.5f, 1,0,
            -0.5f, -0.5f, -0.5f, 0,0,
            -0.5f,  0.5f, -0.5f, 0,1,
             0.5f,  0.5f, -0.5f, 1,1,

            // Left face
            -0.5f, -0.5f, -0.5f, 0,0,
            -0.5f, -0.5f,  0.5f, 1,0,
            -0.5f,  0.5f,  0.5f, 1,1,
            -0.5f, -0.5f, -0.5f, 0,0,
            -0.5f,  0.5f,  0.5f, 1,1,
            -0.5f,  0.5f, -0.5f, 0,1,

            // Right face
             0.5f, -0.5f, -0.5f, 0,0,
             0.5f,  0.5f,  0.5f, 1,1,
             0.5f, -0.5f,  0.5f, 1,0,
             0.5f, -0.5f, -0.5f, 0,0,
             0.5f,  0.5f, -0.5f, 0,1,
             0.5f,  0.5f,  0.5f, 1,1,

            // Top face
            -0.5f,  0.5f, -0.5f, 0,0,
            -0.5f,  0.5f,  0.5f, 0,1,
             0.5f,  0.5f,  0.5f, 1,1,
            -0.5f,  0.5f, -0.5f, 0,0,
             0.5f,  0.5f,  0.5f, 1,1,
             0.5f,  0.5f, -0.5f, 1,0,

            // Bottom face
            -0.5f, -0.5f, -0.5f, 0,0,
             0.5f, -0.5f,  0.5f, 1,1,
            -0.5f, -0.5f,  0.5f, 0,1,
            -0.5f, -0.5f, -0.5f, 0,0,
             0.5f, -0.5f, -0.5f, 1,0,
             0.5f, -0.5f,  0.5f, 1,1
        };
        _bufferResource = _renderAPI.CreateBuffer(new BufferResourceDescriptor(
            BufferObjectType.Vertex,
            BufferUsageType.Static,
            MemoryMarshal.AsBytes(vertices.AsSpan()).ToArray()
        ));

        _cubeTransforms = new Matrix4x4[10000];
        var rng = new Random();
        for (int i = 0; i < 10000; i++)
        {
            float x = (float)(rng.NextDouble() * 100 - 50);
            float y = (float)(rng.NextDouble() * 100 - 50);
            float z = (float)(rng.NextDouble() * 100 - 50);
            _cubeTransforms[i] = Matrix4x4.CreateTranslation(x, y, z);
        }

        _pipelineResource = _renderAPI.CreatePipeline(new PipelineResourceDescriptor() { BlendingMode = BlendingMode.AlphaBlend, CullMode = CullMode.Back, DepthTest = true, StencilTest = false });
    }

    private float _cameraAngle = 0f;
    private float _cameraDistance = 20f;
    Matrix4x4 _viewMatrix;

    public void UpdateCamera(float deltaTime)
    {
        _cameraAngle += deltaTime * 0.5f;

        float camX = MathF.Cos(_cameraAngle) * _cameraDistance;
        float camZ = MathF.Sin(_cameraAngle) * _cameraDistance;
        Vector3 cameraPos = new Vector3(camX, 10f, camZ);
        Vector3 target = Vector3.Zero;
        Vector3 up = Vector3.UnitY;

        _viewMatrix = Matrix4x4.CreateLookAt(cameraPos, target, up);
    }

    public void OnDraw(float deltaTime)
    {
        UpdateCamera(deltaTime);

        Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 4,
            (float)_windowService.FrameSize.X / (float)_windowService.FrameSize.Y,
            0.1f, 1000f
        );

        Matrix4x4 viewProj = _viewMatrix * proj;

        _renderAPI.Submit((IRenderContext ctx) =>
        {
            ctx.SetFrameBuffer(null, _windowService.FrameSize.X, _windowService.FrameSize.Y);
            ctx.Clear(Color.WhiteSmoke);
            ctx.SetPipeline(_pipelineResource);
            ctx.SetShader(_shaderResource);
            ctx.SetTexture(0, _textureResource);

            for (int i = 0; i < _cubeTransforms.Length; i++)
            {
                _shaderResource.Set("uModel", _cubeTransforms[i]);
                _shaderResource.Set("uViewProj", viewProj);
                ctx.DrawArrays(_bufferResource, _vertexArrayResource, 36, PrimitiveType.Triangles);
            }
        });
    }
}
