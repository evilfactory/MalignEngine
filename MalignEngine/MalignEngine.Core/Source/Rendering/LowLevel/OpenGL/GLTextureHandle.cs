using CommunityToolkit.HighPerformance;
using Silk.NET.OpenGL;

namespace MalignEngine
{
    public class GLTextureHandle : TextureHandle
    {
        private bool isRenderTarget;
        private GL gl;
        internal uint frameBufferHandle;
        internal uint depthBufferHandle;
        internal uint textureHandle;

        public GLTextureHandle(GL gl)
        {
            unsafe
            {
                this.gl = gl;
            }
        }

        public override void Initialize(int width, int height, bool renderTarget = false)
        {
            isRenderTarget = renderTarget;
            unsafe
            {
                textureHandle = gl.GenTexture();
                if (renderTarget)
                {
                    frameBufferHandle = gl.GenFramebuffer();
                    depthBufferHandle = gl.GenRenderbuffer();
                }

                Resize(width, height);
            }
        }

        public override void SubmitData(Color[] data)
        {
            unsafe
            {
                Span<Color> dataSpan = data.AsSpan();
                fixed (void* d = &dataSpan[0])
                {
                    gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)Width, (uint)Height, PixelFormat.Rgba, PixelType.UnsignedByte, d);
                }
            }
        }

        public override void SubmitData(System.Drawing.Rectangle bounds, byte[] data)
        {
            unsafe
            {
                Bind();
                fixed (byte* ptr = data)
                {
                    gl.TexSubImage2D(
                        target: TextureTarget.Texture2D,
                        level: 0,
                        xoffset: bounds.Left,
                        yoffset: bounds.Top,
                        width: (uint)bounds.Width,
                        height: (uint)bounds.Height,
                        format: PixelFormat.Rgba,
                        type: PixelType.UnsignedByte,
                        pixels: ptr
                    );
                }
            }
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            gl.ActiveTexture(textureSlot);
            gl.BindTexture(TextureTarget.Texture2D, textureHandle);
        }

        public void BindAsRenderTarget()
        {
            gl.BindTexture(TextureTarget.Texture2D, textureHandle);
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferHandle);
            gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBufferHandle);
        }

        public override void Resize(int width, int height)
        {
            Width = width;
            Height = height;

            unsafe
            {
                Bind();
                gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);

                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);

                if (isRenderTarget)
                {
                    gl.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferHandle);
                    gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBufferHandle);

                    gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.DepthComponent, (uint)width, (uint)height);
                    gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthAttachment, GLEnum.Renderbuffer, depthBufferHandle);

                    gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, textureHandle, 0);

                    gl.DrawBuffers(1, new GLEnum[] { GLEnum.ColorAttachment0 });

                    // Reset the framebuffer binding
                    gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                }
                else
                {
                    gl.GenerateMipmap(TextureTarget.Texture2D);
                }
            }
        }
    }
}