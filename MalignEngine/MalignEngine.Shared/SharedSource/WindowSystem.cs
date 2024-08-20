using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Numerics;

namespace MalignEngine
{
    public class WindowSystem : BaseSystem
    {
        public int Width => window.Size.X;
        public int Height => window.Size.Y;

        public event Action<Vector2> OnFrameBufferResize;
        public event Action<Vector2> OnResize;

        internal IWindow window;
        private SystemGroup systemGroup;

        public WindowSystem(SystemGroup group, string title, Vector2 size)
        {
            systemGroup = group;

            var options = WindowOptions.Default;
            options.PreferredDepthBufferBits = 8;
            options.PreferredStencilBufferBits = 8;
            options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(4, 1));
            options.WindowBorder = WindowBorder.Resizable;
            options.Size = new Vector2D<int>((int)size.X, (int)size.Y);
            options.Title = title;
            window = Window.Create(options);

            window.UpdatesPerSecond = 60;
            window.FramesPerSecond = 120;
            window.VSync = false;

            window.Load += WindowLoad; ;
            window.Update += WindowUpdate;
            window.Render += WindowRender;
            window.Resize += WindowResize;
            window.FramebufferResize += WindowFrameBufferResize;
        }

        public void Run()
        {
            window.Run();
        }

        private void WindowLoad()
        {
            systemGroup.Initialize();
        }

        private void WindowUpdate(double delta)
        {
            systemGroup.Update((float)delta);
        }

        private void WindowRender(double delta)
        {
            systemGroup.Draw((float)delta);
        }
        private void WindowResize(Vector2D<int> size)
        {
            OnResize?.Invoke(new Vector2(size.X, size.Y));
        }
        private void WindowFrameBufferResize(Vector2D<int> size)
        {
            OnFrameBufferResize?.Invoke(new Vector2(size.X, size.Y));
        }
    }
}