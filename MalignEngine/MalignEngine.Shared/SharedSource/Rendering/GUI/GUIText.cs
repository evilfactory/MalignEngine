using System.Numerics;

namespace MalignEngine
{
    public class GUIText : GUIComponent
    {
        public GUIStyle Style = GUIStyle.Default;
        public Color Color { get; private set; } = new Color(1f, 1f, 1f, 1f);

        public int FontSize { get; private set; } = 12;
        public string Text { get; private set; }

        public GUIText(RectTransform transform, string text, Color color) : base(transform)
        {
            Color = color;
            Text = text;
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
            RenderingSystem.DrawTexture2D(Style.FrameTexture, RectTransform.TopLeft + RectTransform.ScaledSize / 2f, RectTransform.ScaledSize, Vector2.Zero, new Rectangle(), Color, 0f, 0f);

            FontSystem.DrawFont(Style.NormalFont, 100, Text, RectTransform.TopLeft, Color, 0, new Vector2(0, 0), new Vector2(1f, 1f));

            base.Draw();
        }
    }
}