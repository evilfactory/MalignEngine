using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MalignEngine
{
    public class RenderingSystem : BaseSystem
    {
        [Dependency]
        protected GraphicsDevice GraphicsDevice = default!;

        private SpriteBatch spriteBatch;

        public override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public void ClearColor(Color color)
        {
            GraphicsDevice.Clear(color);
        }

        public void Begin()
        {
            spriteBatch.Begin();
        }

        public void End()
        {
            spriteBatch.End();
        }

        public void Draw(Texture2D texture, Vector2 position, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, float layerDepth)
        {
            spriteBatch.Draw(texture.Texture, position, sourceRectangle, color, rotation, origin, scale, 0, layerDepth);
        }
    }
}