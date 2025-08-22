using System.Numerics;
using System.Runtime.InteropServices;

namespace MalignEngine;

public struct VertexPositionColorTexture
{
    public Vector3 Position;
    public Color Color;
    public Vector2 TextureCoordinate;

    public VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate)
    {
        Position = position;
        Color = color;
        TextureCoordinate = textureCoordinate;
    }
}

public interface IRenderer2D : IService
{
    public bool FlipY { get; set; }
    public void Begin(IRenderContext renderContext, Matrix4x4 matrix, Material material = null);
    public void Begin(IRenderContext renderContext, Material material = null);
    public void End();
    public void DrawTexture2D(ITextureResource texture, Vector2 position, Vector2 scale, Vector2 uv1, Vector2 uv2, Color color, float rotation, float layerDepth);
    public void DrawTexture2D(ITextureResource texture, Vector2 position, Vector2 scale, Color color, float rotation, float layerDepth);
    public void DrawTexture2D(ITextureResource texture, Vector2 position, Vector2 scale, float layerDepth);
    public void DrawQuad(ITextureResource texture, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture topLeft);
    public void SetMatrix(Matrix4x4 matrix);
}

public class Renderer2D : IRenderer2D
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 UV;
        public float TextureIndex;
        public Color Color;

        public Vertex(Vector3 positon, Vector2 uv, float textureIndex, Color color)
        {
            Position = positon;
            UV = uv;
            TextureIndex = textureIndex;
            Color = color;
        }
    }

    private IRenderingAPI _renderAPI;
    private ILogger _logger;

    private const uint MaxBatchCount = 1000;
    private const uint MaxIndexCount = MaxBatchCount * 6;
    private const uint MaxTextures = 16;

    private IBufferResource _quadVertexBuffer;
    private IBufferResource _quadElementBuffer;

    private IVertexArrayResource _vertexArray;

    private ITextureResource[] _textures;

    private uint _indexCount = 0;
    private uint _batchIndex = 0;

    private Vertex[] _batchVertices;
    private uint[] _triangleIndices;

    private bool _drawing = false;
    private Material? _drawingMaterial;
    private Matrix4x4 _drawingMatrix;
    private IRenderContext? _renderContext;
    private Matrix4x4 _currentMatrix;

    private IShaderResource _basicShader;
    private Material _basicMaterial;

    public bool FlipY { get; set; }

    public Renderer2D(ILoggerService loggerService, IRenderingAPI renderAPI)
    {
        _logger = loggerService.GetSawmill("rendering.2d");
        _renderAPI = renderAPI;
        _textures = new ITextureResource[MaxTextures];

        _triangleIndices = new uint[MaxIndexCount];
        uint offset = 0;

        for (uint i = 0; i < MaxIndexCount; i += 6)
        {
            _triangleIndices[i + 0] = 0 + offset;
            _triangleIndices[i + 1] = 1 + offset;
            _triangleIndices[i + 2] = 3 + offset;

            _triangleIndices[i + 3] = 1 + offset;
            _triangleIndices[i + 4] = 2 + offset;
            _triangleIndices[i + 5] = 3 + offset;

            offset += 4;
        }

        _batchVertices = new Vertex[MaxBatchCount * 4];

        _quadElementBuffer = _renderAPI.CreateBuffer(new BufferResourceDescriptor(BufferObjectType.Element, BufferUsageType.Static, MemoryMarshal.AsBytes(_triangleIndices.AsSpan()).ToArray()));
        _quadVertexBuffer = _renderAPI.CreateBuffer(new BufferResourceDescriptor(BufferObjectType.Vertex, BufferUsageType.Static, MemoryMarshal.AsBytes(_batchVertices.AsSpan()).ToArray()));

        IVertexArrayDescriptor descriptor = new VertexArrayDescriptor();
        descriptor.AddAttribute("Position", 0, VertexAttributeType.Float, 3);
        descriptor.AddAttribute("UV", 1, VertexAttributeType.Float, 2);
        descriptor.AddAttribute("TexIndex", 2, VertexAttributeType.Float, 1);
        descriptor.AddAttribute("Color", 3, VertexAttributeType.UnsignedByte, 4);

        _vertexArray = _renderAPI.CreateVertexArray(descriptor);
        _currentMatrix = Matrix4x4.Identity;

        _basicShader = _renderAPI.CreateShader(new ShaderResourceDescriptor()
        {
            FragmentShaderSource = """
            #version 330 core
            in vec2 fUv;
            in float fTexIndex;
            in vec4 fColor;

            uniform sampler2D uTextures[16];

            out vec4 FragColor;

            void main()
            {
            	int index = int(fTexIndex);
            	vec4 texColor = vec4(1.0, 1.0, 1.0, 1.0);

            	switch (index) { // glsl apparently doesn't support dynamic indexing :thumbsup:
            	case 0:
            		texColor = texture(uTextures[0], fUv);
            		break;
            	case 1:
            		texColor = texture(uTextures[1], fUv);
            		break;
            	case 2:
            		texColor = texture(uTextures[2], fUv);
            		break;
            	case 3:
            		texColor = texture(uTextures[3], fUv);
            		break;
            	case 4:
            		texColor = texture(uTextures[4], fUv);
            		break;
            	case 5:
            		texColor = texture(uTextures[5], fUv);
            		break;
            	case 6:
            		texColor = texture(uTextures[6], fUv);
            		break;
            	case 7:
            		texColor = texture(uTextures[7], fUv);
            		break;
            	case 8:
            		texColor = texture(uTextures[8], fUv);
            		break;
            	case 9:
            		texColor = texture(uTextures[9], fUv);
            		break;
            	case 10:
            		texColor = texture(uTextures[10], fUv);
            		break;
            	case 11:
            		texColor = texture(uTextures[11], fUv);
            		break;
            	case 12:
            		texColor = texture(uTextures[12], fUv);
            		break;
            	case 13:
            		texColor = texture(uTextures[13], fUv);
            		break;
            	case 14:
            		texColor = texture(uTextures[14], fUv);
            		break;
            	case 15:
            		texColor = texture(uTextures[15], fUv);
            		break;
            	}

            	if (texColor.a < 0.1)
            	{
            		discard;
            	}

            	FragColor = texColor * (fColor / 255);
            }
            """,
            VertexShaderSource = """
            #version 330 core
            layout (location = 0) in vec3 vPos;
            layout (location = 1) in vec2 vUv;
            layout (location = 2) in float vTexIndex;
            layout (location = 3) in vec4 vColor;

            uniform mat4 uModel;
            uniform mat4 uView;
            uniform mat4 uProjection;

            out vec2 fUv;
            out float fTexIndex;
            out vec4 fColor;

            void main()
            {
                gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
                fUv = vUv;
                fTexIndex = vTexIndex;
            	fColor = vColor;
            }
            """
        });

        _basicMaterial = new Material(_basicShader);
    }

    public void SetMatrix(Matrix4x4 matrix)
    {
        _currentMatrix = matrix;
    }

    public void Begin(IRenderContext renderContext, Matrix4x4 matrix, Material material = null)
    {
        _drawing = true;
        _drawingMaterial = material ?? _basicMaterial;
        _drawingMatrix = matrix;
        _renderContext = renderContext;

        _textures = new ITextureResource[MaxTextures];

        _batchIndex = 0;
    }

    public void Begin(IRenderContext renderContext, Material material = null)
    {
        Begin(renderContext, _currentMatrix, material);
    }

    public void DrawTexture2D(ITextureResource texture, Vector2 position, Vector2 size, Vector2 uv1, Vector2 uv2, Color color, float rotation, float layerDepth)
    {
        var rotationQ = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rotation);
        var rotationMatrix = Matrix4x4.CreateFromQuaternion(rotationQ);
        var translationMatrix = Matrix4x4.CreateTranslation(new Vector3(position, layerDepth));
        var transform = rotationMatrix * translationMatrix;

        var halfSize = size / 2f;
        Vector3 topRight = new Vector3(halfSize.X, halfSize.Y, 0f);
        Vector3 bottomRight = new Vector3(halfSize.X, -halfSize.Y, 0f);
        Vector3 bottomLeft = new Vector3(-halfSize.X, -halfSize.Y, 0f);
        Vector3 topLeft = new Vector3(-halfSize.X, halfSize.Y, 0f);

        topRight = Vector3.Transform(topRight, transform);
        bottomRight = Vector3.Transform(bottomRight, transform);
        bottomLeft = Vector3.Transform(bottomLeft, transform);
        topLeft = Vector3.Transform(topLeft, transform);

        DrawQuad(texture,
            new VertexPositionColorTexture(topRight, color, new Vector2(uv2.X, uv2.Y)), // top right 1,1
            new VertexPositionColorTexture(bottomRight, color, new Vector2(uv2.X, uv1.Y)), // bottom right 1,0
            new VertexPositionColorTexture(bottomLeft, color, new Vector2(uv1.X, uv1.Y)), // bottom left 0,0
            new VertexPositionColorTexture(topLeft, color, new Vector2(uv1.X, uv2.Y))  // top left 0,1
        );
    }

    public void DrawTexture2D(ITextureResource texture, Vector2 position, Vector2 size, Color color, float rotation, float layerDepth)
    {
        DrawTexture2D(texture, position, size, Vector2.Zero, Vector2.One, color, rotation, layerDepth);
    }

    public void DrawTexture2D(ITextureResource texture, Vector2 position, Vector2 size, float layerDepth)
    {
        DrawTexture2D(texture, position, size, Vector2.Zero, Vector2.One, Color.White, 0f, layerDepth);
    }

    public void DrawQuad(ITextureResource texture, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture topLeft)
    {
        DrawVertices(texture, new VertexPositionColorTexture[] { topRight, bottomRight, bottomLeft, topLeft });
    }

    public void DrawVertices(ITextureResource texture, VertexPositionColorTexture[] vertices)
    {
        if (!_drawing)
        {
            throw new InvalidOperationException("Begin must be called before Draw.");
        }

        if (texture == null) { throw new ArgumentNullException(nameof(texture)); }

        // Don't batch draw multiple textures if texture batching is disabled
        if (_indexCount >= MaxIndexCount || (texture != _textures[0]))
        {
            End();
            Begin(_renderContext, _drawingMatrix, _drawingMaterial);
        }

        int textureSlot = -1;

        for (int i = 0; i < _textures.Length; i++)
        {
            if (texture == null) { break; }

            if (_textures[i] == null)
            {
                _textures[i] = texture;
                textureSlot = i;
                break;
            }

            if (_textures[i] == texture)
            {
                textureSlot = i;
                break;
            }
        }

        if (FlipY)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].TextureCoordinate.Y = 1f - vertices[i].TextureCoordinate.Y;
            }
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            _batchVertices[_batchIndex] = new Vertex(vertices[i].Position, vertices[i].TextureCoordinate, textureSlot, vertices[i].Color);
            _batchIndex++;
        }

        uint amountQuads = (uint)vertices.Length / 4;

        _indexCount += amountQuads * 6;
    }

    public void End()
    {
        if (!_drawing)
        {
            throw new Exception("Called End() before Begin()");
        }

        _quadVertexBuffer.BufferData(_batchVertices, 0, _batchIndex);

        IShaderResource drawingShader = _drawingMaterial.Shader;

        _renderContext.SetShader(drawingShader);

        drawingShader.Set("uModel", Matrix4x4.Identity);
        drawingShader.Set("uView", Matrix4x4.Identity);
        drawingShader.Set("uProjection", _drawingMatrix);

        drawingShader.Set("uTextures", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

        // First 16 textures are reserved for the texture batching
        int textureIndex = (int)MaxTextures;
        foreach (var property in _drawingMaterial.Properties)
        {
            object propertyValue = property.Value;

            if (propertyValue is int)
            {
                drawingShader.Set(property.Key, (int)propertyValue);
            }
            else if (propertyValue is float)
            {
                drawingShader.Set(property.Key, (float)propertyValue);
            }
            else if (propertyValue is Color)
            {
                drawingShader.Set(property.Key, (Color)propertyValue);
            }
            else if (propertyValue is Matrix4x4)
            {
                drawingShader.Set(property.Key, (Matrix4x4)propertyValue);
            }
            else if (propertyValue is ITextureResource)
            {
                _renderContext.SetTexture(textureIndex, (ITextureResource)propertyValue);
                drawingShader.Set(property.Key, textureIndex);
                textureIndex++;
            }
        }

        for (int i = 0; i < _textures.Length; i++)
        {
            if (_textures[i] == null) { break; }

            _renderContext.SetTexture(i, _textures[i]);
        }

        _renderContext.DrawIndexed(_quadElementBuffer, _quadVertexBuffer, _vertexArray, _indexCount);

        _indexCount = 0;
        _drawing = false;
    }
}