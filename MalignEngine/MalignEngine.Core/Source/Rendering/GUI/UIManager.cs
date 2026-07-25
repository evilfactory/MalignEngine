using System.Numerics;

namespace MalignEngine;

public class UIManager : BaseSystem, ITextInput, IKeyPressed, IKeyReleased
{
    public Widget Root { get; }

    private Widget? hoveredWidget;
    private Widget? focusedWidget;

    private readonly IUIPainter _uiPainter;
    private readonly IRenderingAPI _renderAPI;
    private readonly IRenderer2D _render2D;
    private readonly IWindowService _windowService;
    private readonly IInputService _inputService;

    public UIManager(IServiceContainer serviceContainer, IWindowService windowService, IInputService inputService, IRenderingAPI renderAPI, IUIPainter uiPainter, IRenderer2D render2D) : base(serviceContainer)
    {
        Root = new Container() { IsHitTestVisibile = true };

        _windowService = windowService;
        _uiPainter = uiPainter;
        _renderAPI = renderAPI;
        _render2D = render2D;
        _inputService = inputService;
    }

    public Widget? HitTest(Widget widget, Vector2 mouse)
    {
        for (int i = widget.Children.Count - 1; i >= 0; i--)
        {
            if (!widget.Children[i].IsVisible)
            {
                continue;
            }

            Widget? hit = HitTest(widget.Children[i], mouse);

            if (hit != null && hit.IsHitTestVisibile)
            {
                return hit;
            }
        }

        if (widget.IsVisible && widget.IsHitTestVisibile && widget.HitTest(mouse))
        {
            return widget;
        }

        return null;
    }

    public void Focus(Widget? widget)
    {
        if (focusedWidget != null)
        {
            focusedWidget.OnFocusLost();
        }

        focusedWidget = widget;

        if (widget != null)
        {
            widget.OnFocusGained();
        }
    }

    public override void OnDraw(float deltaTime)
    {
        _renderAPI.Submit(ctx =>
        {
            Root.CalculateMeasure(new Vector2(_windowService.FrameSize.X, _windowService.FrameSize.Y));
            Root.Arrange(new RectangleF(0f, 0f, _windowService.FrameSize.X, _windowService.FrameSize.Y));

            _render2D.Begin(ctx, Matrix4x4.CreateOrthographicOffCenter(0f, _windowService.FrameSize.X, _windowService.FrameSize.Y, 0f, 0.001f, 100f));
            Root.Draw(_uiPainter);

            if (hoveredWidget != null)
            {
                _uiPainter.DrawRect(hoveredWidget.Bounds, Color.DeepPink, 5f);
            }

            if (focusedWidget != null)
            {
                _uiPainter.DrawRect(focusedWidget.Bounds, Color.Blue, 5f);
            }

            _render2D.End();
        });
    }

    public override void OnUpdate(float deltaTime)
    {
        var newHoveredWidget = HitTest(Root, _inputService.Mouse.Position);

        if (newHoveredWidget != hoveredWidget)
        {
            hoveredWidget?.OnMouseLeave();
            newHoveredWidget?.OnMouseEnter();

            hoveredWidget = newHoveredWidget;
        }

        if (_inputService.Mouse.IsPressed(MouseButton.Left))
        {
            hoveredWidget?.OnClick(MouseButton.Left);
            Focus(hoveredWidget);
        }

        if (_inputService.Mouse.IsDown(MouseButton.Left))
        {
            hoveredWidget?.OnMousePressed(MouseButton.Left);
            Focus(hoveredWidget);
        }

        if (_inputService.Mouse.IsReleased(MouseButton.Left))
        {
            hoveredWidget?.OnMouseReleased(MouseButton.Left);
            Focus(hoveredWidget);
        }
    }

    public void OnTextInput(char input)
    {
        if (focusedWidget != null)
        {
            focusedWidget.OnTextInput(input);
        }
    }

    public void OnKeyPressed(Key key)
    {
        if (focusedWidget != null)
        {
            focusedWidget.OnKeyPressed(key);
        }
    }

    public void OnKeyReleased(Key key)
    {
        if (focusedWidget != null)
        {
            focusedWidget.OnKeyReleased(key);
        }
    }
}