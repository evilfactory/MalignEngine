using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Numerics;

namespace MalignEngine
{
    public class WindowSystem : BaseSystem, IApplicationRun
    {
        [Dependency]
        protected EventSystem EventSystem = default!;

        public int Width => window.Size.X;
        public int Height => window.Size.Y;

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

        public void OnApplicationRun()
        {
            window.Run();
        }

        private void WindowLoad()
        {
            EventSystem.PublishEvent<IInit>(e => e.OnInitialize());
        }

        private void WindowUpdate(double delta)
        {
            EventSystem.PublishEvent<IPreUpdate>(e => e.OnPreUpdate((float)delta));
            EventSystem.PublishEvent<IUpdate>(e => e.OnUpdate((float)delta));
            EventSystem.PublishEvent<IPostUpdate>(e => e.OnPostUpdate((float)delta));
        }

        private void WindowRender(double delta)
        {
            EventSystem.PublishEvent<IWindowDraw>(e => e.OnWindowDraw((float)delta));
        }
    }
}