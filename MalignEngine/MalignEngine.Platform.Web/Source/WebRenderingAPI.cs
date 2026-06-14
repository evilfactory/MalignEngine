using FontStashSharp.RichText;
using nkast.Wasm.Canvas;
using nkast.Wasm.Canvas.WebGL;
using nkast.Wasm.Dom;
using System.Diagnostics;
using System.Text;

namespace MalignEngine;

[Stage<IPreDraw, LowestPriorityStage>]
[Stage<IPostDraw, HighestPriorityStage>]
public class WebRenderingAPI : IRenderingAPI, IPreDraw, IPostDraw, IDisposable
{
    private ILogger _logger;
    private bool _running;

    private Queue<Delegate> _queue = new();

    private readonly IScheduleManager _scheduleManager;

    private IRenderContext? _context;

    [Dependency]
    protected IPerformanceProfiler? _performanceProfiler;

    private Canvas canvas;
    private IWebGL2RenderingContext _gl;

    public WebRenderingAPI(IScheduleManager scheduleManager, ILoggerService loggerService)
    {
        _running = true;

        canvas = Window.Current.Document.GetElementById<Canvas>("game");

        ContextAttributes attribs = new ContextAttributes();
        attribs.Depth = true;
        _gl = canvas.GetContext<IWebGL2RenderingContext>(attribs);

        _scheduleManager = scheduleManager;
        _logger = loggerService.GetSawmill("rendering.api");

        _scheduleManager.RegisterAll(this);

        //_gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        _gl.Enable(WebGLCapability.BLEND);
        _gl.DepthFunc(WebGLDepthComparisonFunc.LEQUAL);
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

        _queue.Enqueue(command);
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
        using (_performanceProfiler?.BeginSample("rendering.api.renderthread.frame"))
        {
            _context = new WebRenderContext(_gl);

            while (_queue.TryDequeue(out Delegate command))
            {
                ExecuteRenderCommand(command);
            }


            // swap buffers
        }
    }

    public bool IsInRenderingThread()
    {
        return true;
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

    public IPipelineResource CreatePipeline(IPipelineResourceDescriptor descriptor)
    {
        return new GLPipelineResource(_gl, descriptor);
    }

    public void Dispose()
    {
        _running = false;

        _scheduleManager.UnregisterAll(this);

        _logger.LogInfo("WebRenderingAPI disposed");
    }

    public void OnPreDraw(float deltaTime)
    {
        BeginFrame();
    }

    public void OnPostDraw(float deltaTime)
    {
        EndFrame();
    }
}