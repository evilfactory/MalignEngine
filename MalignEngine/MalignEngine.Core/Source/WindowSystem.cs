using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Numerics;

namespace MalignEngine
{
    public class WindowSystem : BaseSystem, IApplicationRun, IUpdateLoop
    {
        [Dependency]
        protected ScheduleManager EventSystem = default!;

        public double UpdateRate
        {
            get { return window.UpdatesPerSecond; }
            set { window.UpdatesPerSecond = value; }
        }

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
            EventSystem.Run<IInit>(e => e.OnInitialize());
        }

        private void WindowUpdate(double delta)
        {
            EventSystem.Run<IPreUpdate>(e => e.OnPreUpdate((float)delta));
            EventSystem.Run<IUpdate>(e => e.OnUpdate((float)delta));
            EventSystem.Run<IPostUpdate>(e => e.OnPostUpdate((float)delta));
        }

        private void WindowRender(double delta)
        {
            EventSystem.Run<IWindowDraw>(e => e.OnWindowDraw((float)delta));
        }
    }
}