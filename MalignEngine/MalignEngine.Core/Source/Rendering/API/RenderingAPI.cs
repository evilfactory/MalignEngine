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
    None, AlphaBlend, Additive
}

public enum CullMode
{
    None,
    Back,
    Front
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

public delegate void RenderCommand();
public delegate void RenderCommandContext(IRenderContext context);

public interface IRenderingAPI : IGpuResourceFactory, IService
{
    void BeginFrame();
    void Submit(RenderCommand command);
    void Submit(RenderCommandContext command);
    void EndFrame();
    bool IsInRenderingThread();
}

public interface IRenderContext
{
    // Draw commands
    void Clear(Color color);
    void DrawIndexed(IBufferResource indexBuffer, IBufferResource vertexBuffer, IVertexArrayResource vertexArray, uint indices, PrimitiveType primitiveType = PrimitiveType.Triangles);
    void DrawArrays(IBufferResource vertexBuffer, IVertexArrayResource vertexArray, uint count, PrimitiveType primitiveType = PrimitiveType.Triangles);

    // Binding commands
    void SetPipeline(IPipelineResource pipeline);
    void SetFrameBuffer(IFrameBufferResource framebuffer, int width = 0, int height = 0);
    void SetShader(IShaderResource shader);
    void SetTexture(int slot, ITextureResource texture);
}