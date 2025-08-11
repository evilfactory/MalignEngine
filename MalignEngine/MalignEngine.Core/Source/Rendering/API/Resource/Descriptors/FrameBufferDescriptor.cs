namespace MalignEngine;

public interface IFrameBufferDescriptor
{
    public int ColorAttachmentCount { get; }
    public int Width { get; }
    public int Height { get; }
}

public class FrameBufferDescriptor : IFrameBufferDescriptor
{
    public int ColorAttachmentCount { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public FrameBufferDescriptor(int colorAttachmentCount, int width, int height)
    {
        ColorAttachmentCount = colorAttachmentCount;
        Width = width;
        Height = height;
    }
}
