using FontStashSharp;

namespace MalignEngine;

public static class LayoutHelper
{
    public static float ResolveWidth(RectangleF slot, Widget child, float desired)
    {
        return child.Width.Unit switch
        {
            LengthUnit.Pixels => child.Width.Value,
            LengthUnit.Percent => slot.Width * child.Width.Value,
            LengthUnit.Auto => desired,
            LengthUnit.Fill => slot.Width,
            _ => desired
        };
    }

    public static float ResolveHeight(RectangleF slot, Widget child, float desired)
    {
        return child.Height.Unit switch
        {
            LengthUnit.Pixels => child.Height.Value,
            LengthUnit.Percent => slot.Height * child.Height.Value,
            LengthUnit.Auto => desired,
            LengthUnit.Fill => slot.Height,
            _ => desired
        };
    }

    public static float ResolveHorizontal(RectangleF slot, Widget child, float width)
    {
        return child.HorizontalAlignment switch
        {
            HorizontalAlignment.Left =>
                slot.Left,

            HorizontalAlignment.Center =>
                slot.Left + (slot.Width - width) * 0.5f,

            HorizontalAlignment.Right =>
                slot.Right - width,

            _ => slot.Left
        };
    }

    public static float ResolveVertical(RectangleF slot, Widget child, float height)
    {
        return child.VerticalAlignment switch
        {
            VerticalAlignment.Top =>
                slot.Top,

            VerticalAlignment.Center =>
                slot.Top + (slot.Height - height) * 0.5f,

            VerticalAlignment.Bottom =>
                slot.Bottom - height,

            _ => slot.Top
        };
    }
}