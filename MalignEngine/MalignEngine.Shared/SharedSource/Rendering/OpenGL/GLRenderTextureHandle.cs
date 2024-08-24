using Silk.NET.OpenGL;

namespace MalignEngine
{
    public class GLRenderTextureHandle : RenderTextureHandle, IGLBindableTexture
    {
        private GL gl;
        internal uint frameBufferHandle;
        internal uint depthBufferHandle;
        internal uint textureHandle;

        public GLRenderTextureHandle(GL gl, uint width, uint height) : base(width, height)
        {
            this.gl = gl;

            unsafe
            {
                frameBufferHandle = gl.GenFramebuffer();
                depthBufferHandle = gl.GenRenderbuffer();
                textureHandle = gl.GenTexture();
            }

            Resize(width, height);
        }

        public void Bind()
        {
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferHandle);
            gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBufferHandle);
            gl.BindTexture(TextureTarget.Texture2D, textureHandle);
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            gl.ActiveTexture(textureSlot);
            gl.BindTexture(TextureTarget.Texture2D, textureHandle);
        }

        public override void Resize(uint width, uint height)
        {
            Width = width;
            Height = height;

            unsafe
            {
                Bind();
                gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);

                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);

                gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.DepthComponent, width, height);
                gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthAttachment, GLEnum.Renderbuffer, depthBufferHandle);

                gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, textureHandle, 0);

                gl.DrawBuffers(1, new GLEnum[] { GLEnum.ColorAttachment0 });

            }
        }
    }
}