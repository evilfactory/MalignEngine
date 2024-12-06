using System.Numerics;

namespace MalignEngine
{
    public class Sprite
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Origin { get; private set; }
        public Rectangle Rect { get; private set; }
        public Color Color { get; private set; }

        public Sprite(Texture2D texture)
        {
            Texture = texture;
            Origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Rect = new Rectangle(0, 0, (int)texture.Width, (int)texture.Height);
            Color = Color.White;
        }

        public Sprite(Texture2D texture, Vector2 origin, Rectangle rect)
        {
            Texture = texture;
            Origin = origin;
            Rect = rect;
            Color = Color.White;
        }

        public Sprite(Texture2D texture, Vector2 origin, Rectangle rect, Color color)
        {
            Texture = texture;
            Origin = origin;
            Rect = rect;
            Color = color;
        }

        public override string ToString()
        {
            return $"Sprite: ({Texture.Width}x{Texture.Height})";
        }
    }
}