using System.Numerics;
using System.Runtime.InteropServices.JavaScript;

namespace MalignEngine;

public class ButtonStyle : Style
{
    public required TextStyle TextStyle { get; set; }
    public required Color Color { get; set; }
    public required Color HoverColor { get; set; }
    public required Color ClickColor { get; set; }
}

public class Button : Widget
{
    private readonly ButtonStyle _style;

    public Border Border { get; }
    public TextBlock TextBlock { get; }

    public string Text
    {
        get => TextBlock.Text;
        set => TextBlock.Text = value;
    }

    private Color currentColor;
    private Color targetColor;

    public Button(ButtonStyle style)
    {
        _style = style;

        Border = new Border
        {
            Color = style.Color,
            Padding = new Thickness(8)
        };

        TextBlock = new TextBlock(style.TextStyle)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        Border.AddChild(TextBlock);
        AddChild(Border);

        currentColor = style.Color;
        targetColor = style.Color;

        IsHitTestVisibile = true;
    }

    protected override Vector2 Measure(Vector2 availableSize)
    {
        return Border.DesiredSize;
    }

    public override void Arrange(RectangleF bounds)
    {
        Bounds = bounds;

        Border.Arrange(bounds);
    }

    public override void Draw(IUIPainter painter)
    {
        currentColor = Color.Lerp(currentColor, targetColor, 0.1f);

        Border.Color = currentColor;

        base.Draw(painter);
    }

    public override void OnClick(MouseButton button)
    {
        currentColor = _style.ClickColor;
    }

    public override void OnMouseEnter()
    {
        targetColor = _style.HoverColor;
    }

    public override void OnMouseLeave()
    {
        targetColor = _style.Color;
    }
}