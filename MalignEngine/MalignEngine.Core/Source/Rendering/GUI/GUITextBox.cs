using System.Numerics;

namespace MalignEngine;

/*
public class GUITextBox : GUIComponent, IKeyboardSubscriber
{
    public GUIStyle Style { get; set; }

    public int FontSize { get; set; } = 100;
    public string Text { get; set; }

    private bool isFocused = false;

    public GUITextBox(RectTransform transform) : this(transform, GUIStyle.Default) { }
    public GUITextBox(RectTransform transform, GUIStyle style) : base(transform)
    {
        Style = style;
    }

    public override void Update()
    {
        base.Update();

        if (InputSystem.IsMouseButtonPressed(0))
        {
            if (InputSystem.MousePosition.X > RectTransform.TopLeft.X && InputSystem.MousePosition.X < RectTransform.TopLeft.X + RectTransform.ScaledSize.X &&
                InputSystem.MousePosition.Y > RectTransform.TopLeft.Y && InputSystem.MousePosition.Y < RectTransform.TopLeft.Y + RectTransform.ScaledSize.Y)
            {
                isFocused = true;
            }
            else
            {
                isFocused = false;
            }
        }
    }

    public override void Draw()
    {
        IRenderingService.DrawTexture2D(Style.FrameTexture, RectTransform.TopLeft + RectTransform.ScaledSize / 2f, RectTransform.ScaledSize, isFocused ? Style.BoxFocusedColor : Style.BoxUnfocusedColor, 0f, 0f);
        FontSystem.DrawFont(Style.NormalFont, FontSize, Text, RectTransform.TopLeft + RectTransform.ScaledSize / 2f - Style.NormalFont.MeasureText(Text, FontSize) / 2f, Color.White, 0, new Vector2(0, 0), new Vector2(1f, 1f));

        base.Draw();
    }

    public void OnKeyPress(Key key)
    {
        if (isFocused)
        {
            if (key == Key.Backspace && Text.Length > 0)
            {
                Text = Text.Remove(Text.Length - 1);
            }
        }
    }
    public void OnKeyRelease(Key key) { }

    public void OnCharEntered(char character)
    {
        if (isFocused)
        {
            Text += character;
        }
    }
}
*/