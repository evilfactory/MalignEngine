using System.Numerics;

namespace MalignEngine;

public class TextFieldStyle : Style
{
    public required Color Color { get; set; }
    public required TextStyle TextStyle { get; set; }
    public required Color CursorColor { get; set; }
}

public class TextField : Widget
{
    private readonly TextFieldStyle _style;

    public Border Border { get; }
    public TextBlock TextBlock { get; }

    public string Text { get; private set; } = "";

    private int cursorPosition;

    private bool focused;

    public TextField(TextFieldStyle style)
    {
        _style = style;

        Border = new Border
        {
            Color = style.Color,
            Padding = new Thickness(8)
        };

        TextBlock = new TextBlock(style.TextStyle);

        Border.AddChild(TextBlock);
        AddChild(Border);

        IsHitTestVisibile = true;
    }

    protected override Vector2 Measure(Vector2 availableSize)
    {
        Border.CalculateMeasure(availableSize);

        return Border.DesiredSize;
    }

    public override void Arrange(RectangleF bounds)
    {
        Bounds = bounds;

        Border.Arrange(bounds);
    }

    public override void Draw(IUIPainter painter)
    {
        base.Draw(painter);

        if (focused)
        {
            Vector2 textPosition = TextBlock.GetTextPosition();

            float xPos = textPosition.X + TextBlock.EffectiveFont.MeasureText(Text.Substring(0, cursorPosition), TextBlock.EffectiveFontSize).X;
            painter.FillRect(new RectangleF(xPos, textPosition.Y, 2f, TextBlock.EffectiveFontSize), _style.CursorColor);
        }
    }

    public override void OnFocusGained()
    {
        focused = true;
    }

    public override void OnFocusLost()
    {
        focused = false;
    }

    public override void OnTextInput(char c)
    {
        if (!focused)
        {
            return;
        }

        Text = Text.Insert(cursorPosition, c.ToString());
        cursorPosition++;

        TextBlock.Text = Text;
    }

    public override void OnKeyPressed(Key key)
    {
        if (!focused)
        {
            return;
        }

        switch (key)
        {
            case Key.Backspace:
                if (cursorPosition > 0)
                {
                    Text = Text.Remove(cursorPosition - 1, 1);
                    cursorPosition--;
                    TextBlock.Text = Text;
                }
                break;

            case Key.LeftArrow:
                cursorPosition = Math.Max(0, cursorPosition - 1);
                break;

            case Key.RightArrow:
                cursorPosition = Math.Min(Text.Length, cursorPosition + 1);
                break;
        }
    }
}