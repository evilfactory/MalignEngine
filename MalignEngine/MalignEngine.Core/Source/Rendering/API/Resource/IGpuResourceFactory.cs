namespace MalignEngine;

public interface IGpuResourceFactory
{
    IBufferResource CreateBuffer(IBufferResourceDescriptor descriptor);
    IVertexArrayResource CreateVertexArray(IVertexArrayDescriptor descriptor);
    ITextureResource CreateTexture(ITextureDescriptor descriptor);
    IShaderResource CreateShader(IShaderResourceDescriptor descriptor);
    IFrameBufferResource CreateFrameBuffer(IFrameBufferDescriptor descriptor);
}