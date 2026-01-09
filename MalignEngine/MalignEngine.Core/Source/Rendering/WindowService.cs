using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using System.Numerics;

namespace MalignEngine;

public interface IWindowContextProvider
{
    public IWindow GetWindow();
}

public interface IWindowService
{
    string Title { get; set; }
    Vector2D<int> Size { get; set; }
    Vector2D<int> FrameSize { get; }
    void ClearContext();
    void MakeContextCurrent();
    void SwapBuffers();
}

public class WindowService : BaseSystem, IWindowService, IWindowContextProvider, IPreUpdate
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

    public Vector2D<int> FrameSize
    {
        get
        {
            return window.FramebufferSize;
        }
    }

    public int Width => Size.X;
    public int Height => Size.Y;

    // for renderer, i need to get rid of this later
    internal IWindow window;

    public WindowService(IServiceContainer serviceContainer)
        : base (serviceContainer)
    {
        var options = WindowOptions.Default;
        options.PreferredDepthBufferBits = 8;
        options.PreferredStencilBufferBits = 8;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(4, 1));
        options.WindowBorder = WindowBorder.Resizable;
        options.Size = new Vector2D<int>(1280, 1280);
        options.Title = "Malign Engine";
        window = Window.Create(options);

        window.VSync = false;

        window.Initialize();

        Logger.LogInfo($"Window \"{Title}\" initialized {options.Size.X}x{options.Size.Y}");
    }

    public IWindow GetWindow() => window;

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

    public override void Dispose()
    {
        base.Dispose();

        Logger.LogInfo($"Window \"{Title}\" destroyed");

        window.Dispose();
    }

    public void OnPreUpdate(float deltaTime)
    {
        window.DoEvents();

        if (window.IsClosing)
        {
            ScheduleManager.Run<IApplicationClosing>(x => x.OnApplicationClosing());
        }
    }
}