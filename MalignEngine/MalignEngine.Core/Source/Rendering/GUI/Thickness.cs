public struct Thickness
{
    public float Left;
    public float Top;
    public float Right;
    public float Bottom;

    public Thickness(float uniform)
    {
        Left = Top = Right = Bottom = uniform;
    }

    public Thickness(float horizontal, float vertical)
    {
        Left = Right = horizontal;
        Top = Bottom = vertical;
    }

    public Thickness(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
}