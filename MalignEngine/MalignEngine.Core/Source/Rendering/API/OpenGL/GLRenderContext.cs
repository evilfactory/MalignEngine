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

    public void SetFrameBuffer(IFrameBufferResource frameBufferResource, int width = 0, int height = 0)
    {
        if (frameBufferResource != null)
        {
            if (width == 0) { width = frameBufferResource.Width; }
            if (height == 0) { height = frameBufferResource.Height; }
            ((GLFrameBufferResource)frameBufferResource).Bind();
            _gl.Viewport(0, 0, (uint)width, (uint)height);
        }
        else
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Viewport(0, 0, (uint)width, (uint)height);
        }
    }

    public void SetTexture(int slot, ITextureResource texture)
    {
        ((GLTextureResource)texture).Bind(TextureUnit.Texture0 + slot);
    }

    public void SetStencil(StencilFunction function, int reference, uint mask, StencilOperation fail, StencilOperation zfail, StencilOperation zpass)
    {
        GLEnum functiongl = GLEnum.Always;

        switch (function)
        {
            case StencilFunction.Equal:
                functiongl = GLEnum.Equal;
                break;
            case StencilFunction.NotEqual:
                functiongl = GLEnum.Notequal;
                break;
            case StencilFunction.Less:
                functiongl = GLEnum.Less;
                break;
            case StencilFunction.LessThanOrEqual:
                functiongl = GLEnum.Lequal;
                break;
            case StencilFunction.Greater:
                functiongl = GLEnum.Greater;
                break;
            case StencilFunction.GreaterThanOrEqual:
                functiongl = GLEnum.Gequal;
                break;
            case StencilFunction.Always:
                functiongl = GLEnum.Always;
                break;
            case StencilFunction.Never:
                functiongl = GLEnum.Never;
                break;
        }

        GLEnum OperationToGl(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Keep:
                    return GLEnum.Keep;
                case StencilOperation.Zero:
                    return GLEnum.Zero;
                case StencilOperation.Replace:
                    return GLEnum.Replace;
                case StencilOperation.Increment:
                    return GLEnum.Incr;
                case StencilOperation.IncrementWrap:
                    return GLEnum.IncrWrap;
                case StencilOperation.Decrement:
                    return GLEnum.Decr;
                case StencilOperation.DecrementWrap:
                    return GLEnum.DecrWrap;
                case StencilOperation.Invert:
                    return GLEnum.Invert;
                default:
                    return GLEnum.Keep;
            }
        }

        _gl.StencilOp(OperationToGl(fail), OperationToGl(zfail), OperationToGl(zpass));
        _gl.StencilFunc(functiongl, reference, mask);
    }

    public void SetBlendingMode(BlendingMode mode)
    {
        switch (mode)
        {
            case BlendingMode.AlphaBlend:
                _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
                break;
            case BlendingMode.Additive:
                _gl.BlendFunc(GLEnum.One, GLEnum.One);
                break;
        }
    }
}
