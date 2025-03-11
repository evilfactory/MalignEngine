
using Microsoft.Extensions.Logging;
using Silk.NET.Core.Native;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using Silk.NET.Windowing;
using System.Diagnostics;

namespace MalignEngine;

public class GLRenderingAPI : IRenderingAPI, IInit
{
    private ILogger logger;

    private WindowService window;

    private GL gl;

    public GLRenderingAPI(WindowService window, ILoggerService loggerService)
    {
        this.window = window;
        logger = loggerService.GetSawmill("rendering.api");
    }

    public void OnInitialize()
    {
        gl = GL.GetApi(window.window);

        gl.Enable(GLEnum.Blend);
        gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        gl.Enable(GLEnum.DepthTest);
        gl.Enable(GLEnum.StencilTest);

        gl.DepthFunc(GLEnum.Lequal);

        gl.Enable(GLEnum.DebugOutput);
        gl.Enable(GLEnum.DebugOutputSynchronous);
        unsafe
        {
            gl.DebugMessageCallback(GLDebugMessageCallback, null);
        }
    }

    private void GLDebugMessageCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
    {
        string stringMessage = SilkMarshal.PtrToString(message);

        switch (severity)
        {
            case GLEnum.DebugSeverityHigh:
                logger.LogError($"{id}: {type} of {severity}, raised from {source}: {stringMessage}");
                break;
            case GLEnum.DebugSeverityMedium:
                logger.LogWarning($"{id}: {type} of {severity}, raised from {source}: {stringMessage}");
                break;
            case GLEnum.DebugSeverityLow:
            case GLEnum.DebugSeverityNotification:
            default:
                logger.LogVerbose($"{id}: {type} of {severity}, raised from {source}: {stringMessage}");
                break;
        }

        Debug.Assert(false);
    }

    public void Clear(Color color)
    {
        gl.ClearColor(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
    }

    public BufferObject<TBufferType> CreateBuffer<TBufferType>(Span<TBufferType> data, BufferObjectType type, BufferUsageType usage) where TBufferType : unmanaged
    {
        return new GLBufferObject<TBufferType>(gl, data, type, usage);
    }

    public Shader CreateShader(Stream data)
    {
        StreamReader reader = new StreamReader(data);
        return new GLShader(gl, reader.ReadToEnd());
    }

    public TextureHandle CreateTextureHandle()
    {
        return new GLTextureHandle(gl);
    }

    public VertexArrayObject CreateVertexArray()
    {
        return new GLVertexArrayObject(gl);
    }

    public void DrawIndexed<TVertex>(BufferObject<uint> indexBuffer, BufferObject<TVertex> vertexBuffer, VertexArrayObject vertexArray, uint indices) where TVertex : unmanaged
    {
        GLVertexArrayObject vao = (GLVertexArrayObject)vertexArray;
        GLBufferObject<TVertex> vbo = (GLBufferObject<TVertex>)vertexBuffer;
        GLBufferObject<uint> ibo = (GLBufferObject<uint>)indexBuffer;

        vao.Bind();
        vbo.Bind();
        ibo.Bind();

        unsafe
        {
            gl.DrawElements(PrimitiveType.Triangles, indices, DrawElementsType.UnsignedInt, null);
        }
    }

    public void SetRenderTarget(RenderTexture renderTexture, int width = 0, int height = 0)
    {
        if (renderTexture != null)
        {
            if (width == 0) { width = renderTexture.Width; }
            if (height == 0) { height = renderTexture.Height; }
            ((GLTextureHandle)renderTexture.Handle).BindAsRenderTarget();
            gl.Viewport(0, 0, (uint)width, (uint)height);
        }
        else
        {
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            gl.Viewport(0, 0, (uint)window.Width, (uint)window.Height);
        }
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

        gl.StencilOp(OperationToGl(fail), OperationToGl(zfail), OperationToGl(zpass));
        gl.StencilFunc(functiongl, reference, mask);
    }
}