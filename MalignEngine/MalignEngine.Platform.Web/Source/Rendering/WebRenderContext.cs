using nkast.Wasm.Canvas.WebGL;

namespace MalignEngine;

public class WebRenderContext : IRenderContext
{
    private IWebGL2RenderingContext _gl;

    internal WebRenderContext(IWebGL2RenderingContext gl)
    {
        _gl = gl;
    }

    private WebGLPrimitiveType PrimitiveTypeToGlType(PrimitiveType type)
    {
        switch (type)
        {
            case PrimitiveType.Triangles:
                return WebGLPrimitiveType.TRIANGLES;
            case PrimitiveType.Lines:
                return WebGLPrimitiveType.LINES;
            case PrimitiveType.Points:
                throw new NotImplementedException("Not supported");
            default:
                return WebGLPrimitiveType.TRIANGLES;
        }
    }

    public void Clear(Color color)
    {
        _gl.ClearColor(color.R, color.G, color.B, color.A);
        _gl.Clear(WebGLBufferBits.COLOR | WebGLBufferBits.DEPTH | WebGLBufferBits.STENCIL);
    }

    public void DrawIndexed(IBufferResource indexBuffer, IBufferResource vertexBuffer, IVertexArrayResource vertexArray, uint indices, PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        WebVertexArrayResource vao = (WebVertexArrayResource)vertexArray;
        WebBufferResource vbo = (WebBufferResource)vertexBuffer;
        WebBufferResource ibo = (WebBufferResource)indexBuffer;

        vao.Bind();
        vbo.Bind();
        ibo.Bind();

        _gl.DrawElements(PrimitiveTypeToGlType(primitiveType), (int)indices, WebGLDataType.UINT, 0);
    }

    public void DrawArrays(IBufferResource vertexBuffer, IVertexArrayResource vertexArray, uint count, PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        WebVertexArrayResource vao = (WebVertexArrayResource)vertexArray;
        WebBufferResource vbo = (WebBufferResource)vertexBuffer;
        vao.Bind();
        vbo.Bind();
        _gl.DrawArrays(PrimitiveTypeToGlType(primitiveType), 0, (int)count);
    }

    public void SetShader(IShaderResource shader)
    {
        WebShaderResource glShader = (WebShaderResource)shader;
        glShader.Use();
    }

    public void SetFrameBuffer(IFrameBufferResource? frameBufferResource, int width = 0, int height = 0)
    {
        if (frameBufferResource == null)
        {
            _gl.BindFramebuffer(WebGL2FramebufferType.FRAMEBUFFER, null);
            _gl.Viewport(0, 0, width, height);
        }
        else if (frameBufferResource is WebFrameBufferResource webFrameBuffer)
        {
            if (width == 0) { width = frameBufferResource.Width; }
            if (height == 0) { height = frameBufferResource.Height; }
            webFrameBuffer.Bind();
            _gl.Viewport(0, 0, width, height);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public void SetTexture(int slot, ITextureResource? texture)
    {
        if (texture == null)
        {
            _gl.ActiveTexture(WebGLTextureUnit.TEXTURE0 + slot);
            _gl.BindTexture(WebGLTextureTarget.TEXTURE_2D, null);
        }
        else if (texture is WebTextureResource webTexture)
        {
            webTexture.Bind(WebGLTextureUnit.TEXTURE0 + slot);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public void SetPipeline(IPipelineResource pipeline)
    {
        ((WebPipelineResource)pipeline).Bind();
    }
}
