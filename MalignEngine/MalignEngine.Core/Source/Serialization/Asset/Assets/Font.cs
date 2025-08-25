using FontStashSharp;
using System.Numerics;

namespace MalignEngine;

public class Font : IAsset
{
    internal FontStashSharp.FontSystem fontSystem = new FontStashSharp.FontSystem();

    public Font(byte[] data)
    {
        fontSystem.AddFont(data);
    }

    public Vector2 MeasureText(string text, int size)
    {
        return fontSystem.GetFont(size).MeasureString(text);
    }
}