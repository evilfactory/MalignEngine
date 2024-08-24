using CommunityToolkit.HighPerformance;
using Silk.NET.OpenGL;
using System.IO;

namespace MalignEngine
{
    public interface IGLBindableTexture
    {
        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0);
    }

    public class GLTextureHandle : TextureHandle, IGLBindableTexture
    {
        private GL gl;
        internal uint handle;

        public GLTextureHandle(GL gl, uint width, uint height) : base(width, height)
        {
            unsafe
            {
                this.gl = gl;
                handle = gl.GenTexture();

                Bind();
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);

                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);

                gl.GenerateMipmap(TextureTarget.Texture2D);
            }
        }

        public override void SubmitData(Color[] data)
        {
            unsafe
            {
                Span<Color> dataSpan = data.AsSpan();
                fixed (void* d = &dataSpan[0])
                {
                    gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, PixelFormat.Rgba, PixelType.UnsignedByte, d);
                }
            }
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            gl.ActiveTexture(textureSlot);
            gl.BindTexture(TextureTarget.Texture2D, handle);
        }
    }
}