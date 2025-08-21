using Microsoft.Extensions.Logging;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using System.Diagnostics;
using System.Text;

namespace MalignEngine;

public class GLRenderingAPI : IRenderingAPI, IDisposable
{
    public GL InternalAPI => _gl;

    private GL _gl;
    private ILogger _logger;
    private WindowService _window;
    private Thread _renderThread;
    private bool _running;

    private readonly AutoResetEvent _frameReady = new(false);
    private readonly AutoResetEvent _frameComplete = new(true);

    private Queue<Delegate> _frontQueue = new();
    private Queue<Delegate> _backQueue = new();

    private IRenderContext _context;

    [Dependency]
    protected IPerformanceProfiler _performanceProfiler;

    public GLRenderingAPI(WindowService window, ILoggerService loggerService)
    {
        _running = true;

        _window = window;
        _logger = loggerService.GetSawmill("rendering.api");

        _gl = GL.GetApi(_window.window);
        _window.ClearContext();

        _renderThread = new Thread(RenderThread);

        _renderThread.Name = "Render Thread";

        _renderThread.Start();
    }

    private void GLDebugMessageCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
    {
        string stringMessage = SilkMarshal.PtrToString(message);

        switch (severity)
        {
            case GLEnum.DebugSeverityHigh:
                _logger.LogError($"{id}: {type} of {severity}, raised from {source}: {stringMessage}");
                Debug.Assert(true);
                break;
            case GLEnum.DebugSeverityMedium:
                _logger.LogWarning($"{id}: {type} of {severity}, raised from {source}: {stringMessage}");
                break;
            case GLEnum.DebugSeverityLow:
            case GLEnum.DebugSeverityNotification:
            default:
                _logger.LogVerbose($"{id}: {type} of {severity}, raised from {source}: {stringMessage}");
                break;
        }

    }

    private void RenderThread()
    {
        _window.MakeContextCurrent();

        _gl.Enable(GLEnum.Blend);
        _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        //_gl.Enable(GLEnum.DepthTest);
        //gl.Enable(GLEnum.CullFace);
        //gl.CullFace(GLEnum.Back);
        //_gl.Enable(GLEnum.StencilTest);

        _gl.DepthFunc(GLEnum.Lequal);

        _gl.Enable(GLEnum.DebugOutput);
        _gl.Enable(GLEnum.DebugOutputSynchronous);
        unsafe
        {
            _gl.DebugMessageCallback(GLDebugMessageCallback, null);
        }

        StringBuilder extensions = new StringBuilder();
        int numExtensions = _gl.GetInteger(GLEnum.NumExtensions);
        for (int i = 0; i < numExtensions; i++)
        {
            extensions.Append(_gl.GetStringS(GLEnum.Extensions, (uint)i));
            extensions.Append(" ");
        }

        _logger.LogInfo($"Initialized OpenGL. \n OpenGL {_gl.GetStringS(GLEnum.Version)}\n Shading Language {_gl.GetStringS(GLEnum.ShadingLanguageVersion)} \n GPU: {_gl.GetStringS(GLEnum.Renderer)} \n Vendor: {_gl.GetStringS(GLEnum.Vendor)}");

        while (_running)
        {
            using (_performanceProfiler.BeginSample("rendering.api.renderthread.waiting.frame"))
            {
                _frameReady.WaitOne();
            }

            using (_performanceProfiler.BeginSample("rendering.api.renderthread.frame"))
            {
                _context = new GLRenderContext(_gl);

                while (_frontQueue.TryDequeue(out Delegate command))
                {
                    ExecuteRenderCommand(command);
                }

                if (_window.window.IsInitialized)
                {
                    _window.SwapBuffers();
                }
            }

            _frameComplete.Set();
        }

        _logger.LogInfo("render thread ran to completion");
    }

    public void BeginFrame()
    {

    }

    private void ExecuteRenderCommand(Delegate command)
    {
        if (command is RenderCommand renderCommand)
        {
            renderCommand();
        }
        else if (command is RenderCommandContext renderCommandContext)
        {
            renderCommandContext(_context);
        }
    }

    private void SubmitInternal(Delegate command)
    {
        if (IsInRenderingThread())
        {
            ExecuteRenderCommand(command);
            return;
        }

        _backQueue.Enqueue(command);
    }

    public void Submit(RenderCommand command)
    {
        SubmitInternal(command);
    }

    public void Submit(RenderCommandContext command)
    {
        SubmitInternal(command);
    }

    public void EndFrame()
    {
        using (_performanceProfiler.BeginSample("rendering.api.waiting.prevframe"))
        {
            _frameComplete.WaitOne();
        }

        _frontQueue = _backQueue;
        _backQueue = new Queue<Delegate>();

        _frameReady.Set();
    }

    public bool IsInRenderingThread()
    {
        return Thread.CurrentThread == _renderThread;
    }

    public IBufferResource CreateBuffer(IBufferResourceDescriptor descriptor)
    {
        return new GLBufferResource(_gl, this, descriptor);
    }

    public IVertexArrayResource CreateVertexArray(IVertexArrayDescriptor descriptor)
    {
        return new GLVertexArrayResource(_gl, this, descriptor);
    }

    public ITextureResource CreateTexture(ITextureDescriptor descriptor)
    {
        return new GLTextureResource(_gl, this, descriptor);
    }

    public IShaderResource CreateShader(IShaderResourceDescriptor descriptor)
    {
        return new GLShaderResource(_gl, this, descriptor);
    }

    public IFrameBufferResource CreateFrameBuffer(IFrameBufferDescriptor descriptor)
    {
        return new GLFrameBufferResource(_gl, this, descriptor);
    }

    public void Dispose()
    {
        _running = false;
        _frameReady.Set();
        _renderThread.Join();

        _logger.LogInfo("disposing");
    }
}