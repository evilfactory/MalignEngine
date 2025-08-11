namespace MalignEngine;

public interface IFrameBufferResource : IGpuResource
{
    int Width { get; }
    int Height { get; }
    int ColorAttachmentCount { get; }
    ITextureResource GetColorAttachment(int index);
    ITextureResource? DepthAttachment { get; }
    void Resize(int width, int height);
}