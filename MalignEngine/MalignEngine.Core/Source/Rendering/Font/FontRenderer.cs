using FontStashSharp.Interfaces;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace MalignEngine;

public interface IFontRenderer
{
    public void DrawFont(Font font, int fontSize, string text, Vector2 position, Color color) => DrawFont(font, fontSize, text, position, color, 0f, Vector2.Zero, Vector2.One);
    public void DrawFont(Font font, int fontSize, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale);
}

public class FontRenderer : IFontRenderer, IService, IFontStashRenderer2
{
    public ITexture2DManager TextureManager { get; private set; }
    private IRenderer2D _renderer2D;

    public FontRenderer(IRenderingAPI renderApi, IRenderer2D renderer2D)
    {
        _renderer2D = renderer2D;
        TextureManager = new Texture2DManager(renderApi);
    }

    public void DrawQuad(object texture, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture topLeft)
    {
        _renderer2D.DrawQuad((ITextureResource)texture, topRight, bottomRight, bottomLeft, topLeft);
    }

    public void DrawFont(Font font, int fontSize, string text, Vector2 position, Color color) => DrawFont(font, fontSize, text, position, color, 0f, Vector2.Zero, Vector2.One);

    public void DrawFont(Font font, int fontSize, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale)
    {
        // move texture

        //var dynFont = font.fontSystem.GetFont(fontSize);

        //Vector2 size = dynFont.MeasureString(text, scale);

        //position = new Vector2(position.X, position.Y - size.Y);

        if (scale.Y < 0f) { _renderer2D.FlipY = false; }
        font.fontSystem.GetFont(fontSize).DrawText(this, text, position, new FontStashSharp.FSColor(color.R, color.G, color.B, color.A), rotation, origin, scale);
        if (scale.Y < 0f) { _renderer2D.FlipY = true; }
    }

    public void DrawQuad(object texture, ref FontStashSharp.Interfaces.VertexPositionColorTexture topLeft, ref FontStashSharp.Interfaces.VertexPositionColorTexture topRight, ref FontStashSharp.Interfaces.VertexPositionColorTexture bottomLeft, ref FontStashSharp.Interfaces.VertexPositionColorTexture bottomRight)
    {
        _renderer2D.DrawQuad((ITextureResource)texture,
            new VertexPositionColorTexture(new Vector3(topRight.Position.X, topRight.Position.Y, 0f), new Color(topRight.Color.R, topRight.Color.G, topRight.Color.B, topRight.Color.A), new Vector2(topRight.TextureCoordinate.X, topRight.TextureCoordinate.Y)),
            new VertexPositionColorTexture(new Vector3(bottomRight.Position.X, bottomRight.Position.Y, 0f), new Color(bottomRight.Color.R, bottomRight.Color.G, bottomRight.Color.B, bottomRight.Color.A), new Vector2(bottomRight.TextureCoordinate.X, bottomRight.TextureCoordinate.Y)),
            new VertexPositionColorTexture(new Vector3(bottomLeft.Position.X, bottomLeft.Position.Y, 0f), new Color(bottomLeft.Color.R, bottomLeft.Color.G, bottomLeft.Color.B, bottomLeft.Color.A), new Vector2(bottomLeft.TextureCoordinate.X, bottomLeft.TextureCoordinate.Y)),
            new VertexPositionColorTexture(new Vector3(topLeft.Position.X, topLeft.Position.Y, 0f), new Color(topLeft.Color.R, topLeft.Color.G, topLeft.Color.B, topLeft.Color.A), new Vector2(topLeft.TextureCoordinate.X, topLeft.TextureCoordinate.Y))
        );
    }
}