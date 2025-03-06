using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Numerics;

namespace MalignEngine
{
    public class WindowService : IService, IApplicationRun, IUpdateLoop
    {
        private ScheduleManager scheduleManager;

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

        public double UpdateRate
        {
            get { return window.UpdatesPerSecond; }
            set { window.UpdatesPerSecond = value; }
        }

        public int Width => window.Size.X;
        public int Height => window.Size.Y;

        internal IWindow window;

        public WindowService(ILoggerService loggerService, ScheduleManager scheduleManager)
        {
            this.scheduleManager = scheduleManager;

            var options = WindowOptions.Default;
            options.PreferredDepthBufferBits = 8;
            options.PreferredStencilBufferBits = 8;
            options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(4, 1));
            options.WindowBorder = WindowBorder.Resizable;
            options.Size = new Vector2D<int>(1280, 800);
            options.Title = "Malign Engine";
            window = Window.Create(options);

            window.UpdatesPerSecond = 60;
            window.FramesPerSecond = 120;
            window.VSync = false;

            window.Load += WindowLoad; ;
            window.Update += WindowUpdate;
            window.Render += WindowRender;

            loggerService.GetSawmill("window").LogInfo($"Window initialized {options.Size.X}x{options.Size.Y}");
        }

        public void OnApplicationRun()
        {
            window.Run();
        }

        private void WindowLoad()
        {
            scheduleManager.Run<IInit>(e => e.OnInitialize());
        }

        private void WindowUpdate(double delta)
        {
            scheduleManager.Run<IPreUpdate>(e => e.OnPreUpdate((float)delta));
            scheduleManager.Run<IUpdate>(e => e.OnUpdate((float)delta));
            scheduleManager.Run<IPostUpdate>(e => e.OnPostUpdate((float)delta));
        }

        private void WindowRender(double delta)
        {
            scheduleManager.Run<IWindowDraw>(e => e.OnWindowDraw((float)delta));
        }
    }
}