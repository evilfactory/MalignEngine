using System.Numerics;
using System.Drawing;
using MalignEngine;

namespace MalignEngine
{
    public abstract class GUIComponent
    {
        public RectTransform RectTransform { get; private set; }

        protected FontSystem FontSystem => Application.Main.ServiceContainer.GetInstance<FontSystem>();
        protected IRenderingService IRenderingService => Application.Main.ServiceContainer.GetInstance<IRenderingService>();
        protected InputSystem InputSystem => Application.Main.ServiceContainer.GetInstance<InputSystem>();

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


        public virtual void Update()
        {
            foreach (var child in Children)
            {
                child.Update();
            }
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