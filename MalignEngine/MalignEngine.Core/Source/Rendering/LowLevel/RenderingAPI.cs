using Silk.NET.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MalignEngine;

public enum StencilOperation
{
    Keep,
    Zero,
    Replace,
    Increment,
    IncrementWrap,
    Decrement,
    DecrementWrap,
    Invert
}

public enum StencilFunction
{
    Never,
    Less,
    Equal,
    LessThanOrEqual,
    Greater,
    NotEqual,
    GreaterThanOrEqual,
    Always
}

public enum BlendingMode
{
    AlphaBlend, Additive
}

public enum PrimitiveType
{
    Points = 0,
    Lines = 1,
    LineLoop = 2,
    LineStrip = 3,
    Triangles = 4,
    TriangleStrip = 5,
    TriangleFan = 6
}

public interface IRenderingAPI : IService
{
    public void Clear(Color color);
    public void DrawIndexed<TVertex>(BufferObject<uint> indexBuffer, BufferObject<TVertex> vertexBuffer, VertexArrayObject vertexArray, uint indices, PrimitiveType primitiveType = PrimitiveType.Triangles) where TVertex : unmanaged;
    public void DrawArrays<TVertex>(BufferObject<TVertex> vertexBuffer, VertexArrayObject vertexArray, uint count, PrimitiveType primitiveType = PrimitiveType.Triangles) where TVertex : unmanaged;
    public void SetRenderTarget(RenderTexture renderTexture, int width = 0, int height = 0);
    public void SetShader(Shader shader);
    public void SetStencil(StencilFunction function, int reference, uint mask, StencilOperation fail, StencilOperation zfail, StencilOperation zpass);
    public void SetBlendingMode(BlendingMode mode);
    public void SetTexture(ITexture texture, int index);

    // Factory methods
    public BufferObject<TBufferType> CreateBuffer<TBufferType>(Span<TBufferType> data, BufferObjectType type, BufferUsageType usage) where TBufferType : unmanaged;
    public VertexArrayObject CreateVertexArray();
    public abstract TextureHandle CreateTextureHandle();
    public abstract Shader CreateShader(Stream data);
}