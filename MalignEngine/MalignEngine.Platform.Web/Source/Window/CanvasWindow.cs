using nkast.Wasm.Canvas;
using nkast.Wasm.Canvas.WebGL;
using nkast.Wasm.Dom;
using Silk.NET.Maths;
using System.Numerics;

namespace MalignEngine;


public class CanvasWindow : BaseSystem, IWindowService, IPreUpdate
{
    public string Title { get; set; } = "";

    public Vector2D<int> Size
    {
        get
        {
            return new Vector2D<int>(Canvas.Width, Canvas.Height);
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    public Vector2D<int> FrameSize
    {
        get
        {
            return new Vector2D<int>(Canvas.Width, Canvas.Height);
        }
    }

    public int Width => Size.X;
    public int Height => Size.Y;

    public Canvas Canvas { get; private set; }

    public CanvasWindow(IServiceContainer serviceContainer) : base(serviceContainer)
    {
        Canvas = Window.Current.Document.GetElementById<Canvas>("theCanvas");
        ContextAttributes attribs = new ContextAttributes();
        attribs.Depth = true;

        Logger.LogInfo($"Window \"{Title}\" initialized {Canvas.Width}x{Canvas.Height}");
    }

    public void ClearContext()
    {

    }

    public void MakeContextCurrent()
    {
    }

    public void SwapBuffers()
    {

    }

    public override void Dispose()
    {
        base.Dispose();

        Logger.LogInfo($"Window \"{Title}\" destroyed");
    }

    public void OnPreUpdate(float deltaTime)
    {

    }
}