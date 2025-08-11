using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using System.Numerics;

namespace MalignEngine;

public interface IWindowService
{
    string Title { get; set; }
    Vector2D<int> Size { get; set; }
    void ClearContext();
    void MakeContextCurrent();
    void SwapBuffers();
}

public class WindowService : IWindowService, IService, IPreUpdate, IDisposable
{
    public string Title
    {
        get { return window.Title; }
        set {  window.Title = value; }
    }

    public Vector2D<int> Size
    {
        get
        {
            return window.Size;
        }
        set
        {
            window.Size = value;
        }
    }

    public int Width => Size.X;
    public int Height => Size.Y;

    private ILogger _logger;
    private IEventLoop _eventLoop;

    // for renderer, i need to get rid of this later
    internal IWindow window;

    public WindowService(ILoggerService loggerService, IEventLoop eventLoop)
    {
        _logger = loggerService.GetSawmill("window");

        var options = WindowOptions.Default;
        options.PreferredDepthBufferBits = 8;
        options.PreferredStencilBufferBits = 8;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(4, 1));
        options.WindowBorder = WindowBorder.Resizable;
        options.Size = new Vector2D<int>(1280, 800);
        options.Title = "Malign Engine";
        window = Window.Create(options);

        window.VSync = false;

        window.Initialize();

        _eventLoop = eventLoop;

        _logger.LogInfo($"Window \"{Title}\" initialized {options.Size.X}x{options.Size.Y}");
    }

    public void ClearContext()
    {
        window.ClearContext();
    }

    public void MakeContextCurrent()
    {
        window.MakeCurrent();
    }

    public void SwapBuffers()
    {
        window.SwapBuffers();
    }

    public void Dispose()
    {
        _logger.LogInfo($"Window \"{Title}\" destroyed");

        window.Dispose();
    }

    public void OnPreUpdate(float deltaTime)
    {
        window.DoEvents();

        if (window.IsClosing)
        {
            _eventLoop.Stop();
        }
    }
}