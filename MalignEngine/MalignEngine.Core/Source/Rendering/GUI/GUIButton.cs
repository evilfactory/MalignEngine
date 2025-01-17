using System.Numerics;

namespace MalignEngine
{
    public class GUIButton : GUIComponent
    {
        public GUIStyle Style { get; set; }
        public Color DefaultColor { get; set; }
        public Color HoverColor { get; set; }
        public Color ClickColor { get; set; }
        public Action OnClick { get; set; }

        private Color targetColor;
        private Color currentColor;

        public GUIButton(RectTransform transform, Action onClick) : this(transform, GUIStyle.Default, onClick) { }
        public GUIButton(RectTransform transform, GUIStyle style, Action onClick) : base(transform)
        {
            Style = style;
            OnClick = onClick;

            DefaultColor = style.ButtonDefaultColor;
            HoverColor = style.ButtonHoverColor;
            ClickColor = style.ButtonClickColor;

            currentColor = DefaultColor;
        }

        public override void Update()
        {
            base.Update();

            if (InputSystem.MousePosition.X > RectTransform.TopLeft.X && InputSystem.MousePosition.X < RectTransform.TopLeft.X + RectTransform.ScaledSize.X &&
                InputSystem.MousePosition.Y > RectTransform.TopLeft.Y && InputSystem.MousePosition.Y < RectTransform.TopLeft.Y + RectTransform.ScaledSize.Y)
            {
                targetColor = HoverColor;

                if (InputSystem.IsMouseButtonPressed(0))
                {
                    targetColor = ClickColor;
                }
                
                if (InputSystem.IsMouseButtonHeld(0))
                {
                    OnClick();
                }
            }
            else
            {
                targetColor = DefaultColor;
            }

            currentColor = Color.Lerp(currentColor, targetColor, 0.1f);
        }

        public override void Draw()
        {
            RenderingSystem.DrawTexture2D(Style.FrameTexture, RectTransform.TopLeft + RectTransform.ScaledSize / 2f, RectTransform.ScaledSize, currentColor, 0f, 0f);

            base.Draw();
        }
    }
}