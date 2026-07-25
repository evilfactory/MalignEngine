using FontStashSharp;
using MalignEngine;
using System.Numerics;

public enum Orientation
{
    Horizontal,
    Vertical
}

public class Stack : Container
{
    public Orientation Orientation { get; set; } = Orientation.Vertical;
    public float Spacing { get; set; }

    protected override Vector2 Measure(Vector2 availableSize)
    {
        Vector2 desired = Vector2.Zero;

        foreach (Widget child in Children)
        {
            child.CalculateMeasure(availableSize);

            if (Orientation == Orientation.Vertical)
            {
                desired.X = MathF.Max(desired.X, child.DesiredSize.X);
                desired.Y += child.DesiredSize.Y;
            }
            else
            {
                desired.X += child.DesiredSize.X;
                desired.Y = MathF.Max(desired.Y, child.DesiredSize.Y);
            }
        }

        if (Children.Count > 1)
        {
            float spacing = (Children.Count - 1) * Spacing;

            if (Orientation == Orientation.Vertical)
            {
                desired.Y += spacing;
            }
            else
            {
                desired.X += spacing;
            }
        }

        return desired;
    }

    public override void Arrange(RectangleF bounds)
    {
        Bounds = bounds;

        float offset = 0;

        foreach (Widget child in Children)
        {
            RectangleF slot;

            if (Orientation == Orientation.Vertical)
            {
                slot = new RectangleF(bounds.X, bounds.Y + offset, bounds.Width, bounds.Height - offset);
            }
            else
            {
                slot = new RectangleF(bounds.X + offset, bounds.Y, bounds.Width - offset, bounds.Height);
            }

            RectangleF arranged = ArrangeChild(child, slot);

            if (Orientation == Orientation.Vertical)
            {
                offset += arranged.Height + Spacing;
            }
            else
            {
                offset += arranged.Width + Spacing;
            }
        }
    }
}