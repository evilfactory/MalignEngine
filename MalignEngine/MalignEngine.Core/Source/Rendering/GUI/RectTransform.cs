using System;
using System.Numerics;
using System.Drawing;
using System.Reflection;

namespace MalignEngine
{

    public class RectTransform
    {
        public GUIComponent GUIComponent { get; set; }

        public RectTransform Parent { get; private set; }
        public List<RectTransform> Children { get; private set; }

        public Vector2 AbsoluteSize { get; set; }
        public Vector2 AbsoluteOffset { get; set; }
        public Vector2 RelativeSize { get; set; }
        public Anchor Anchor { get; set; }
        public Pivot Pivot { get; set; }

        public Vector2 MinSize { get; set; }
        public Vector2 MaxSize { get; set; }

        public Vector2 TopLeft
        {
            get
            {
                if (Parent == null)
                {
                    return AbsoluteOffset;
                }
                else
                {
                    Vector2 parentSize = Parent.ScaledSize;
                    Vector2 parentPos = Parent.TopLeft;

                    Vector2 pos = new Vector2();

                    switch (Anchor)
                    {
                        case Anchor.TopLeft:
                            pos = new Vector2(parentPos.X, parentPos.Y);
                            break;
                        case Anchor.TopRight:
                            pos = new Vector2(parentPos.X + parentSize.X, parentPos.Y);
                            break;
                        case Anchor.BottomLeft:
                            pos = new Vector2(parentPos.X, parentPos.Y + parentSize.Y);
                            break;
                        case Anchor.BottomRight:
                            pos = new Vector2(parentPos.X + parentSize.X, parentPos.Y + parentSize.Y);
                            break;
                        case Anchor.Center:
                            pos = new Vector2(parentPos.X + parentSize.X / 2, parentPos.Y + parentSize.Y / 2);
                            break;
                        case Anchor.BottomCenter:
                            pos = new Vector2(parentPos.X + parentSize.X / 2, parentPos.Y + parentSize.Y);
                            break;
                        case Anchor.TopCenter:
                            pos = new Vector2(parentPos.X + parentSize.X / 2, parentPos.Y);
                            break;
                    }

                    pos += CalculatePivotOffset(Pivot, ScaledSize);
                    pos += AbsoluteOffset;

                    return pos;
                }
            }
        }

        public Vector2 ScaledSize
        {
            get
            {
                Vector2 size = Vector2.Zero;
                // If the parent is null, then its just the relative size
                if (Parent == null)
                {
                    size = new Vector2(Application.Main.ServiceContainer.GetInstance<WindowService>().Width, Application.Main.ServiceContainer.GetInstance<WindowService>().Height);
                }
                else
                {
                    size = new Vector2(Parent.ScaledSize.X * RelativeSize.X, Parent.ScaledSize.Y * RelativeSize.Y);
                }

                if (MinSize != Vector2.Zero)
                {
                    size = Vector2.Max(size, MinSize);
                }

                if (MaxSize != Vector2.Zero)
                {
                    size = Vector2.Min(size, MaxSize);
                }

                return size;
            }
        }

        public RectTransform(RectTransform parent, Vector2 relativeSize, Anchor anchor, Pivot pivot)
        {
            Parent = parent;
            parent?.AddChild(this);
            Children = new List<RectTransform>();
            RelativeSize = relativeSize;
            Anchor = anchor;
            Pivot = pivot;
        }

        private void AddChild(RectTransform child)
        {
            Children.Add(child);
        }

        public static Vector2 CalculatePivotOffset(Pivot anchor, Vector2 size)
        {
            float width = size.X;
            float height = size.Y;
            switch (anchor)
            {
                case Pivot.TopLeft:
                    return Vector2.Zero;
                case Pivot.TopCenter:
                    return new Vector2(-width / 2, 0);
                case Pivot.TopRight:
                    return new Vector2(-width, 0);
                case Pivot.CenterLeft:
                    return new Vector2(0, -height / 2);
                case Pivot.Center:
                    return -size / 2f;
                case Pivot.CenterRight:
                    return new Vector2(-width, -height / 2);
                case Pivot.BottomLeft:
                    return new Vector2(0, -height);
                case Pivot.BottomCenter:
                    return new Vector2(-width / 2, -height);
                case Pivot.BottomRight:
                    return new Vector2(-width, -height);
                default:
                    throw new NotImplementedException(anchor.ToString());
            }
        }
    }
}