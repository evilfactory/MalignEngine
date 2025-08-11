using Silk.NET.OpenGL;

namespace MalignEngine;

public class GLFrameBufferResource : IFrameBufferResource
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int ColorAttachmentCount { get; private set; }
    public ITextureResource DepthAttachment => depthAttachment;

    private GLTextureResource depthAttachment;

    private GLTextureResource[] textureColorAttachments;
    private GL _gl;
    private IFrameBufferDescriptor _descriptor;
    private GLRenderingAPI _renderAPI;

    private uint _frameHandle;

    public GLFrameBufferResource(GL gl, GLRenderingAPI renderAPI, IFrameBufferDescriptor descriptor)
    {
        _gl = gl;
        _descriptor = descriptor;
        _renderAPI = renderAPI;

        ColorAttachmentCount = _descriptor.ColorAttachmentCount;

        textureColorAttachments = new GLTextureResource[ColorAttachmentCount];

        _renderAPI.Submit(() =>
        {
            _frameHandle = gl.GenFramebuffer();

            for (int i = 0; i < ColorAttachmentCount; i++)
            {
                textureColorAttachments[i] = new GLTextureResource(gl, renderAPI, new TextureDescriptor()
                {
                    GenerateMipmaps = false,
                    Width = descriptor.Width,
                    Height = descriptor.Height,
                });
            }

            depthAttachment = new GLTextureResource(gl, renderAPI, new TextureDescriptor()
            {
                GenerateMipmaps = false,
                Width = descriptor.Width,
                Height = descriptor.Height,
                Format = TextureFormat.Depth24Stencil8
            });
        });

        Resize(descriptor.Width, descriptor.Height);
    }

    public ITextureResource GetColorAttachment(int index)
    {
        if (index < textureColorAttachments.Length)
        {
            return textureColorAttachments[index];
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public void Resize(int width, int height)
    {
        _renderAPI.Submit(() =>
        {

            Width = width;
            Height = height;

            for (int i = 0; i < ColorAttachmentCount; i++)
            {
                textureColorAttachments[i].Resize(width, height);
            }

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _frameHandle);

            _gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.DepthStencilAttachment, GLEnum.Texture2D, depthAttachment.GetHandle(), 0);

            for (int i = 0; i < ColorAttachmentCount; i++)
            {
                _gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0 + i, GLEnum.Texture2D, textureColorAttachments[i].GetHandle(), 0);
            }

            _gl.DrawBuffers(1, new GLEnum[] { GLEnum.ColorAttachment0 });

            var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != GLEnum.FramebufferComplete)
            {
                throw new Exception($"Framebuffer incomplete: {status}");
            }

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        });
    }

    public void Bind()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _frameHandle);
    }

    public void Dispose()
    {

    }
}
