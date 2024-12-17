using FontStashSharp;
using System.Numerics;

namespace MalignEngine
{
    public class Font : IAsset
    {
        public string AssetPath { get; set; }

        internal FontStashSharp.FontSystem fontSystem = new FontStashSharp.FontSystem();

        protected Font(string path)
        {
            fontSystem.AddFont(File.ReadAllBytes(path));
        }

        public Vector2 MeasureText(string text, int size)
        {
            return fontSystem.GetFont(size).MeasureString(text);
        }

        public static IAsset Load(string assetPath)
        {
            Font font = new Font(assetPath);
            font.AssetPath = assetPath;
            return font;
        }
    }
}