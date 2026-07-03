using Silk.NET.OpenGL;

using GLPrimitiveType = Silk.NET.OpenGL.PrimitiveType;

namespace MalignEngine;

public class GLRenderContext : IRenderContext
{
    private GL _gl;

    internal GLRenderContext(GL gl)
    {
        _gl = gl;
    }

    private GLPrimitiveType PrimitiveTypeToGlType(PrimitiveType type)
    {
        switch (type)
        {
            case PrimitiveType.Triangles:
                return GLPrimitiveType.Triangles;
            case PrimitiveType.Lines:
                return GLPrimitiveType.Lines;
            case PrimitiveType.Points:
                return GLPrimitiveType.Points;
            default:
                return GLPrimitiveType.Triangles;
        }
    }

    public void Clear(Color color)
    {
        _gl.ClearColor(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
    }

    public void DrawIndexed(IBufferResource indexBuffer, IBufferResource vertexBuffer, IVertexArrayResource vertexArray, uint indices, PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        GLVertexArrayResource vao = (GLVertexArrayResource)vertexArray;
        GLBufferResource vbo = (GLBufferResource)vertexBuffer;
        GLBufferResource ibo = (GLBufferResource)indexBuffer;

        vao.Bind();
        vbo.Bind();
        ibo.Bind();

        unsafe
        {
            _gl.DrawElements(PrimitiveTypeToGlType(primitiveType), indices, DrawElementsType.UnsignedInt, null);
        }
    }

    public void DrawArrays(IBufferResource vertexBuffer, IVertexArrayResource vertexArray, uint count, PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        GLVertexArrayResource vao = (GLVertexArrayResource)vertexArray;
        GLBufferResource vbo = (GLBufferResource)vertexBuffer;
        vao.Bind();
        vbo.Bind();
        _gl.DrawArrays(PrimitiveTypeToGlType(primitiveType), 0, count);
    }

    public void SetShader(IShaderResource shader)
    {
        GLShaderResource glShader = (GLShaderResource)shader;
        glShader.Use();
    }

    public void SetFrameBuffer(IFrameBufferResource? frameBufferResource, int width = 0, int height = 0)
    {
        if (frameBufferResource == null)
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Viewport(0, 0, (uint)width, (uint)height);
        }
        else if (frameBufferResource is GLFrameBufferResource glFrameBuffer)
        {
            if (width == 0) { width = frameBufferResource.Width; }
            if (height == 0) { height = frameBufferResource.Height; }
            glFrameBuffer.Bind();
            _gl.Viewport(0, 0, (uint)width, (uint)height);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public void SetTexture(int slot, ITextureResource? texture)
    {
        var glSlot = TextureUnit.Texture0 + slot;

        if (texture == null)
        {
            _gl.ActiveTexture(glSlot);
            _gl.BindTexture(TextureTarget.Texture2D, 0);
        }
        else if (texture is GLTextureResource glTexture)
        {
            glTexture.Bind(glSlot);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public void SetPipeline(IPipelineResource pipeline)
    {
        ((GLPipelineResource)pipeline).Bind();
    }
}
