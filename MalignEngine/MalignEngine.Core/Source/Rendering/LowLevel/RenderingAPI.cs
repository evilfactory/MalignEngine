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

public interface IRenderingAPI : IService
{
    public void Clear(Color color);
    public void DrawIndexed<TVertex>(BufferObject<uint> indexBuffer, BufferObject<TVertex> vertexBuffer, VertexArrayObject vertexArray, uint indices) where TVertex : unmanaged;
    public void DrawArrays<TVertex>(BufferObject<TVertex> vertexBuffer, VertexArrayObject vertexArray, uint count) where TVertex : unmanaged;
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