namespace MalignEngine;

using System.Numerics;

public class BorderStyle : Style
{
    public required Color Color { get; set; }
}

public class Border : Container
{
    public Color Color { get; set; }

    public Thickness Padding { get; set; }

    public Border(BorderStyle? style = null)
    {
        if (style != null)
        {
            Color = style.Color;
        }
    }

    protected override Vector2 Measure(Vector2 availableSize)
    {
        Vector2 desired = base.Measure(availableSize);

        desired.X += Padding.Left + Padding.Right;
        desired.Y += Padding.Top + Padding.Bottom;

        return desired;
    }

    public override void Arrange(RectangleF bounds)
    {
        Bounds = bounds;

        RectangleF content = new(bounds.X + Padding.Left, bounds.Y + Padding.Top, bounds.Width - Padding.Left - Padding.Right, bounds.Height - Padding.Top - Padding.Bottom);

        foreach (Widget child in Children)
        {
            ArrangeChild(child, content);
        }
    }

    public override void Draw(IUIPainter painter)
    {
        painter.FillRect(Bounds, Color);

        base.Draw(painter);
    }
}