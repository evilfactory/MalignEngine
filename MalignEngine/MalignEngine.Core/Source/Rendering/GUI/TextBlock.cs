using Cyotek.Drawing.BitmapFont;
using FontStashSharp.RichText;
using System.Numerics;

namespace MalignEngine;

public class TextStyle : Style
{
    public required Font Font { get; set; }
    public required Color TextColor { get; set; }
    public required int FontSize { get; set; }
}

public class TextBlock : Widget
{
    public string Text { get; set; } = "Text";
    public Color? TextColor { get; set; }
    public Font? Font { get; set; }
    public int? FontSize { get; set; }

    private readonly TextStyle _style;

    public Font EffectiveFont => Font ?? _style.Font;
    public Color EffectiveTextColor => TextColor ?? _style.TextColor;
    public int EffectiveFontSize => FontSize ?? _style.FontSize;

    public TextBlock(TextStyle style)
    {
        _style = style;
    }

    public Vector2 GetTextPosition()
    {
        Vector2 size = EffectiveFont.MeasureText(Text, EffectiveFontSize);

        float x = HorizontalAlignment switch
        {
            HorizontalAlignment.Left => Bounds.Left,
            HorizontalAlignment.Center => Bounds.Left + (Bounds.Width - size.X) * 0.5f,
            HorizontalAlignment.Right => Bounds.Right - size.X,
            _ => Bounds.Left
        };

        float y = VerticalAlignment switch
        {
            VerticalAlignment.Top => Bounds.Top,
            VerticalAlignment.Center => Bounds.Top + (Bounds.Height - size.Y) * 0.5f,
            VerticalAlignment.Bottom => Bounds.Bottom - size.Y,
            _ => Bounds.Top
        };

        return new Vector2(x, y);
    }

    public override void Draw(IUIPainter painter)
    {
        Vector2 size = EffectiveFont.MeasureText(Text, EffectiveFontSize);

        painter.DrawText(EffectiveFont, Text, GetTextPosition(), EffectiveFontSize, EffectiveTextColor);

        base.Draw(painter);
    }
}