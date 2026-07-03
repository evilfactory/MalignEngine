using nkast.Wasm.Canvas.WebGL;

namespace MalignEngine;

public class WebFrameBufferResource : IFrameBufferResource
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int ColorAttachmentCount { get; private set; }
    public ITextureResource DepthAttachment => depthAttachment;

    private WebTextureResource depthAttachment;

    private WebTextureResource[] textureColorAttachments;
    private IWebGL2RenderingContext _gl;
    private IFrameBufferDescriptor _descriptor;
    private IRenderingAPI _renderAPI;

    private WebGLFramebuffer _frameHandle;

    public WebFrameBufferResource(IWebGL2RenderingContext gl, IRenderingAPI renderAPI, IFrameBufferDescriptor descriptor)
    {
        _gl = gl;
        _descriptor = descriptor;
        _renderAPI = renderAPI;

        ColorAttachmentCount = _descriptor.ColorAttachmentCount;

        textureColorAttachments = new WebTextureResource[ColorAttachmentCount];

        for (int i = 0; i < ColorAttachmentCount; i++)
        {
            textureColorAttachments[i] = (WebTextureResource)renderAPI.CreateTexture(new TextureDescriptor()
            {
                GenerateMipmaps = false,
                Width = descriptor.Width,
                Height = descriptor.Height,
            });
        }

        depthAttachment = (WebTextureResource)renderAPI.CreateTexture(new TextureDescriptor()
        {
            GenerateMipmaps = false,
            Width = descriptor.Width,
            Height = descriptor.Height,
            Format = TextureFormat.Depth24Stencil8
        });

        _renderAPI.Submit(() =>
        {
            _frameHandle = gl.CreateFramebuffer();
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

            depthAttachment.Resize(width, height);

            _gl.BindFramebuffer(WebGL2FramebufferType.FRAMEBUFFER, _frameHandle);

            _gl.FramebufferTexture2D(WebGL2FramebufferType.FRAMEBUFFER, 
                WebGLFramebufferAttachmentPoint.DEPTH_STENCIL_ATTACHMENT, 
                WebGLTextureTarget.TEXTURE_2D, 
                depthAttachment.WebGLTexture);

            for (int i = 0; i < ColorAttachmentCount; i++)
            {
                _gl.FramebufferTexture2D(WebGL2FramebufferType.FRAMEBUFFER, 
                    WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0, 
                    WebGLTextureTarget.TEXTURE_2D, 
                    textureColorAttachments[i].WebGLTexture);
            }

            _gl.DrawBuffers(new WebGL2DrawBufferAttachmentPoint[] { WebGL2DrawBufferAttachmentPoint .COLOR_ATTACHMENT0 });

            var status = _gl.CheckFramebufferStatus(WebGL2FramebufferType.FRAMEBUFFER);
            if (status != WebGL2FramebufferStatus.FRAMEBUFFER_COMPLETE)
            {
                throw new Exception($"Framebuffer incomplete: {status}");
            }

            _gl.BindFramebuffer(WebGL2FramebufferType.FRAMEBUFFER, null);
        });
    }

    public void Bind()
    {
        _gl.BindFramebuffer(WebGL2FramebufferType.FRAMEBUFFER, _frameHandle);
    }

    public void Dispose()
    {

    }
}
