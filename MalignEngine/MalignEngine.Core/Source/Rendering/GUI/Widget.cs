using System.Numerics;

namespace MalignEngine;

public enum HorizontalAlignment { Left, Center, Right };
public enum VerticalAlignment { Top, Center, Bottom };

public abstract class Widget
{
    public Widget? Parent
    {
        get => _parent;
        set
        {
            if (_parent != null)
            {
                _parent._children.Remove(this);
            }

            if (value != null)
            {
                value._children.Add(this);
            }

            _parent = value;
        }
    }
    public IReadOnlyList<Widget> Children => _children;
    public RectangleF Bounds { get; set; }

    public Length Width { get; set; } = Length.Auto;
    public Length Height { get; set; } = Length.Auto;

    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }

    public bool IsVisible { get; set; } = true;
    public bool IsHitTestVisibile { get; set; } = false;

    public Vector2 DesiredSize { get; private set; }

    private readonly List<Widget> _children = [];
    private Widget? _parent;

    public virtual void Draw(IUIPainter painter)
    {
        foreach (var child in Children)
        {
            child.Draw(painter);
        }
    }

    public virtual bool HitTest(Vector2 position)
    {
        return Bounds.Contains(position);
    }

    public virtual void CalculateMeasure(Vector2 availableSize)
    {
        DesiredSize = Measure(availableSize);
    }

    protected virtual Vector2 Measure(Vector2 availableSize)
    {
        return Vector2.Zero;
    }

    public virtual void Arrange(RectangleF bounds)
    {
        Bounds = bounds;
    }

    public virtual void OnMouseEnter() { }
    public virtual void OnMouseLeave() { }
    public virtual void OnMouseMove(Vector2 position) { }
    public virtual void OnMousePressed(MouseButton button) { }
    public virtual void OnMouseReleased(MouseButton button) { }
    public virtual void OnClick(MouseButton button) { }
    public virtual void OnFocusGained() { }
    public virtual void OnFocusLost() { }
    public virtual void OnTextInput(char c) { }
    public virtual void OnKeyPressed(Key key) { }
    public virtual void OnKeyReleased(Key key) { }
}