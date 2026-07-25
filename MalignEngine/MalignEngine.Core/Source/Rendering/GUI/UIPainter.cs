using nkast.Aether.Physics2D.Dynamics;
using SixLabors.ImageSharp;
using System.Drawing;
using System.Net.Http.Headers;
using System.Numerics;

namespace MalignEngine;

public interface IUIPainter
{
    void FillRect(RectangleF rect, Color color);
    void DrawRect(RectangleF rect, Color color, float thickness);
    void DrawText(Font font, string text, Vector2 position, int fontSize, Color color);
    void DrawImage(Sprite sprite, RectangleF rect);
}

public class UIPainter : IService, IUIPainter
{
    private readonly IRenderingAPI _renderAPI;
    private readonly IRenderer2D _render2D;
    private readonly IFontRenderer _fontRender;

    private ITextureResource _whiteTexture = null!;

    public UIPainter(IRenderingAPI renderAPI, IRenderer2D render2D, IFontRenderer fontRender)
    {
        _renderAPI = renderAPI;
        _render2D = render2D;
        _fontRender = fontRender;

        _renderAPI.Submit(ctx =>
        {
            _whiteTexture = _renderAPI.CreateTexture(new TextureDescriptor(1, 1, TextureFormat.RGBA8) { InitialData = new Color[] { Color.White } });
        });
    }

    public void DrawImage(Sprite sprite, RectangleF rect)
    {
        Vector2 center = new(rect.Left + rect.Width * 0.5f, rect.Top + rect.Height * 0.5f);

        Vector2 size = new(rect.Width, rect.Height);

        _render2D.DrawTexture2D(sprite.Texture.Resource, center, size, sprite.UV1, sprite.UV2, Color.White, 0f, 0f, flipY: true);
    }

    public void FillRect(RectangleF rect, Color color)
    {
        Vector2 center = new(rect.Left + rect.Width * 0.5f, rect.Top + rect.Height * 0.5f);

        Vector2 size = new(rect.Width, rect.Height);

        _render2D.DrawTexture2D(_whiteTexture, center, size, color, 0f, 0f);
    }

    public void DrawRect(RectangleF rect, Color color, float thickness)
    {
        // Top
        FillRect(new RectangleF(rect.Left, rect.Top, rect.Width, thickness), color);

        // Bottom
        FillRect(new RectangleF(rect.Left, rect.Bottom - thickness, rect.Width, thickness), color);

        // Left
        FillRect(new RectangleF(rect.Left, rect.Top + thickness, thickness, rect.Height - thickness * 2), color);

        // Right
        FillRect(new RectangleF(rect.Right - thickness, rect.Top + thickness, thickness, rect.Height - thickness * 2), color);
    }

    public void DrawText(Font font, string text, Vector2 position, int fontSize, Color color)
    {
        _fontRender.DrawFont(font, fontSize, text, position, color);
    }
}