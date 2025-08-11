using System.Numerics;

namespace MalignEngine
{
    /*
    public class GUIText : GUIComponent
    {
        public GUIStyle Style = GUIStyle.Default;
        public Color Color { get; private set; } = new Color(1f, 1f, 1f, 1f);

        public int FontSize { get; private set; } = 50;
        public string Text { get; private set; }

        public GUIText(RectTransform transform, string text, Color color) : base(transform)
        {
            Color = color;
            Text = text;
        }

        public GUIText(RectTransform transform, string text, int fontSize, Color color) : base(transform)
        {
            Color = color;
            Text = text;
            FontSize = fontSize;
        }

        public GUIText(RectTransform transform, string text, GUIStyle style) : base(transform)
        {
            Style = style;
            Text = text;
        }

        public GUIText(RectTransform transform, string text, GUIStyle style, Color color) : base(transform)
        {
            Style = style;
            Color = color;
            Text = text;
        }

        public override void Draw()
        {
            FontSystem.DrawFont(Style.NormalFont, FontSize, Text, RectTransform.TopLeft + RectTransform.ScaledSize / 2f - Style.NormalFont.MeasureText(Text, FontSize) / 2f, Color, 0, new Vector2(0, 0), new Vector2(1f, 1f));

            base.Draw();
        }
    }
    */
}