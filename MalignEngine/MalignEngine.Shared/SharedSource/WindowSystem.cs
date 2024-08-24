using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Numerics;

namespace MalignEngine
{
    public class WindowSystem : BaseSystem
    {
        public int Width => window.Size.X;
        public int Height => window.Size.Y;

        public event Action<float> OnWindowDraw;
        public event Action<float> OnWindowUpdate;

        internal IWindow window;

        public WindowSystem(string title, Vector2 size)
        {
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
        }

        public void Run()
        {
            window.Run();
        }

        private void WindowLoad()
        {
            SystemGroup.Initialize();
        }

        private void WindowUpdate(double delta)
        {
            OnWindowUpdate?.Invoke((float)delta);

            SystemGroup.Update((float)delta);
        }

        private void WindowRender(double delta)
        {
            OnWindowDraw?.Invoke((float)delta);
        }
    }
}