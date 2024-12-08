using FontStashSharp;
using System.Numerics;

namespace MalignEngine
{
    public class Font : IAsset
    {
        internal FontStashSharp.FontSystem fontSystem = new FontStashSharp.FontSystem();

        public Font(string path)
        {
            fontSystem.AddFont(File.ReadAllBytes(path));
        }

        public Vector2 MeasureText(string text, int size)
        {
            return fontSystem.GetFont(size).MeasureString(text);
        }

        public static IAsset Load(string assetPath)
        {
            return new Font(assetPath);
        }
    }
}