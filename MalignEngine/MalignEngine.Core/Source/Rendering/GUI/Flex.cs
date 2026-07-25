using System.Numerics;

namespace MalignEngine;

public class Flex : Container
{
    public Orientation Orientation { get; set; } = Orientation.Vertical;
    public float Spacing { get; set; }

    protected override Vector2 Measure(Vector2 availableSize)
    {
        return base.Measure(availableSize);
    }

    public override void Arrange(RectangleF bounds)
    {
        Bounds = bounds;

        float availableMain = Orientation == Orientation.Vertical ? bounds.Height : bounds.Width;

        float cross = Orientation == Orientation.Vertical ? bounds.Width : bounds.Height;

        float spacing = Math.Max(0, Children.Count - 1) * Spacing;

        float fixedSize = 0f;
        int fillCount = 0;

        foreach (Widget child in Children)
        {
            child.CalculateMeasure(bounds.Size);

            Length length = Orientation == Orientation.Vertical ? child.Height : child.Width;

            switch (length.Unit)
            {
                case LengthUnit.Fill:
                    fillCount++;
                    break;

                case LengthUnit.Percent:
                    fixedSize += availableMain * length.Value;
                    break;

                case LengthUnit.Pixels:
                    fixedSize += length.Value;
                    break;

                case LengthUnit.Auto:
                    fixedSize += Orientation == Orientation.Vertical ? child.DesiredSize.Y : child.DesiredSize.X;
                    break;
            }
        }

        float remaining = Math.Max(0, availableMain - fixedSize - spacing);
        float fillSize = fillCount > 0 ? remaining / fillCount : 0f;

        float offset = 0;

        foreach (Widget child in Children)
        {
            float main = ResolveMainSize(child, fillSize);

            if (Orientation == Orientation.Vertical)
            {
                RectangleF slot = new RectangleF(bounds.X, bounds.Y + offset, bounds.Width, main);
                RectangleF arranged = ArrangeChild(child, slot, height: main);

                offset += arranged.Height + Spacing;
            }
            else
            {
                RectangleF slot = new RectangleF(bounds.X + offset, bounds.Y, main, bounds.Height);
                RectangleF arranged = ArrangeChild(child, slot, width: main);

                offset += arranged.Width + Spacing;
            }
        }
    }

    private float ResolveMainSize(Widget child, float fillSize)
    {
        Length length = Orientation == Orientation.Vertical ? child.Height : child.Width;

        return length.Unit switch
        {
            LengthUnit.Fill => fillSize,
            LengthUnit.Pixels => length.Value,
            LengthUnit.Percent => (Orientation == Orientation.Vertical ? Bounds.Height : Bounds.Width) * length.Value,
            LengthUnit.Auto => Orientation == Orientation.Vertical ? child.DesiredSize.Y : child.DesiredSize.X,
            _ => 0f
        };
    }
}