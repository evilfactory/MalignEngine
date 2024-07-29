using Microsoft.Xna.Framework.Graphics;

namespace MalignEngine
{
    public class Texture2D : Asset
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        internal Microsoft.Xna.Framework.Graphics.Texture2D Texture;

        public Texture2D(string path) : base(path)
        {
        }

        public void LoadFromFile(string filePath)
        {
            Texture = Microsoft.Xna.Framework.Graphics.Texture2D.FromFile(GraphicsApplication.Instance.Game.GraphicsDevice, filePath);
            Width = Texture.Width;
            Height = Texture.Height;
        }

        public static Texture2D CreateFromFile(string filePath)
        {
            Texture2D texture = new Texture2D(filePath);
            texture.LoadFromFile(filePath);
            return texture;
        }

        public override string ToString()
        {
            return $"Texture2D: {Identifier} ({Width}x{Height})";
        }
    }
}