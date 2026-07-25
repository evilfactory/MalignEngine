using System.Numerics;

namespace MalignEngine;

public class Container : Widget
{
    protected RectangleF ArrangeChild(Widget child, RectangleF slot)
    {
        float width = LayoutHelper.ResolveWidth(slot, child, child.DesiredSize.X);
        float height = LayoutHelper.ResolveHeight(slot, child, child.DesiredSize.Y);

        float x = LayoutHelper.ResolveHorizontal(slot, child, width);
        float y = LayoutHelper.ResolveVertical(slot, child, height);

        RectangleF bounds = new(x, y, width, height);

        child.Arrange(bounds);

        return bounds;
    }

    protected override Vector2 Measure(Vector2 availableSize)
    {
        Vector2 desired = Vector2.Zero;

        foreach (Widget child in Children)
        {
            child.CalculateMeasure(availableSize);

            desired.X = MathF.Max(desired.X, child.DesiredSize.X);
            desired.Y = MathF.Max(desired.Y, child.DesiredSize.Y);
        }

        return desired;
    }

    public override void Arrange(RectangleF bounds)
    {
        Bounds = bounds;

        foreach (Widget child in Children)
        {
            ArrangeChild(child, bounds);
        }
    }
}