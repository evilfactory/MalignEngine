using System.Numerics;

namespace MalignEngine;

public class ImageBox : Border
{
    public Sprite? Sprite { get; set; }

    public ImageBox() { }

    public override void Draw(IUIPainter painter)
    {
        if (Sprite != null)
        {
            painter.DrawImage(Sprite, Bounds);
        }

        base.Draw(painter);
    }

    protected override Vector2 Measure(Vector2 availableSize)
    {
        if (Sprite != null)
        {
            return new Vector2(Sprite.Rect.Width, Sprite.Rect.Height);
        }

        return Vector2.Zero;
    }
}