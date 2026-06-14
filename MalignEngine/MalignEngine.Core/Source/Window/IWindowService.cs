using Silk.NET.Maths;
using Silk.NET.Windowing;

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