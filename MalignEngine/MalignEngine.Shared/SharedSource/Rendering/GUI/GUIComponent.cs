using System.Numerics;
using System.Drawing;
using MalignEngine;

namespace MalignEngine
{
    public abstract class GUIComponent
    {
        public RectTransform RectTransform { get; private set; }

        protected FontSystem FontSystem => IoCManager.Resolve<FontSystem>();
        protected RenderingSystem RenderingSystem => IoCManager.Resolve<RenderingSystem>();
        protected InputSystem InputSystem => IoCManager.Resolve<InputSystem>();

        public IEnumerable<GUIComponent> Children
        {
            get
            {
                return RectTransform.Children.Select(x => x.GUIComponent);
            }
        }

        public GUIComponent Parent
        {
            get
            {
                return RectTransform.Parent.GUIComponent;
            }
        }

        public GUIComponent(RectTransform transform)
        {
            RectTransform = transform;
            transform.GUIComponent = this;
        }

        public virtual void Draw()
        {
            // draw children
            foreach (var child in Children)
            {
                child.Draw();
            }
        }
    }
}