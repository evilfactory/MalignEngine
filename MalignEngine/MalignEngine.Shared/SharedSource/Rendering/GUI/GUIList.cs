using System.Numerics;

namespace MalignEngine
{
    public class GUIList : GUIComponent
    {
        public float Spacing { get; set; }

        public GUIList(RectTransform transform, float spacing) : base(transform)
        {
            Spacing = spacing;
        }

        public override void Draw()
        {
            // Before drawing children, we set their AbsoluteOffset to the correct value

            float accumulator = 0;
            foreach (var child in RectTransform.Children)
            {
                accumulator += child.ScaledSize.Y;
                child.AbsoluteOffset = new Vector2(child.AbsoluteOffset.X, accumulator);
                accumulator += Spacing;
            }

            base.Draw();
        }
    }
}