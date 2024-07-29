using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MalignEngine
{
    public class GraphicsApplication : Application
    {
        public static GraphicsApplication? Instance
        {
            get;
            private set;
        }

        public GraphicsDeviceManager GraphicsDeviceManager
        {
            get;
            private set;
        }

        internal MainGame Game;

        public GraphicsApplication() : base()
        {
            Instance = this;

            Game = new MainGame();

            GraphicsDeviceManager = new GraphicsDeviceManager(Game)
            {
                IsFullScreen = false,
            };
            GraphicsDeviceManager.ApplyChanges();

            Game.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            Game.OnInitialized += Initialize;
            Game.OnUpdate += (GameTime gameTime) => Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            Game.OnDraw += (GameTime gameTime) => Draw((float)gameTime.ElapsedGameTime.TotalSeconds);

            IoCManager.Register(Game.GraphicsDevice);
        }

        public override void Run()
        {
            Game.Run();
        }

        public virtual void Draw(float deltaTime)
        {

        }
    }

    internal class MainGame : Game
    {
        public Action? OnInitialized;
        public Action<GameTime>? OnUpdate;
        public Action<GameTime>? OnDraw;

        protected override void Initialize()
        {
            base.Initialize();

            OnInitialized?.Invoke();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            OnUpdate?.Invoke(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            OnDraw?.Invoke(gameTime);
        }
    }
}