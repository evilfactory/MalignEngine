using Silk.NET.Maths;

namespace MalignEngine;

public interface IWindowService
{
    string Title { get; set; }
    Vector2D<int> Size { get; set; }
    Vector2D<int> FrameSize { get; }
    void ClearContext();
    void MakeContextCurrent();
    void SwapBuffers();
}