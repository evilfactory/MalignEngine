using System.Numerics;

namespace MalignEngine
{
    public class GUIFrame : GUIComponent
    {
        public GUIStyle Style = GUIStyle.Default;
        public Color Color { get; private set; } = new Color(1f, 1f, 1f, 1f);

        public GUIFrame(RectTransform transform, Color color) : base(transform)
        {
            Color = color;
        }

        public GUIFrame(RectTransform transform, GUIStyle style) : base(transform)
        {
            Style = style;
        }

        public GUIFrame(RectTransform transform, GUIStyle style, Color color) : base(transform)
        {
            Style = style;
            Color = color;
        }

        public override void Draw()
        {
            RenderingSystem.DrawTexture2D(Style.FrameTexture, RectTransform.TopLeft + RectTransform.ScaledSize / 2f, RectTransform.ScaledSize, Vector2.Zero, new Rectangle(), Color, 0f, 0f);

            base.Draw();
        }
    }
}