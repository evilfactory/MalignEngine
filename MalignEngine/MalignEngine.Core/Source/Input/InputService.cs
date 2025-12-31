using Silk.NET.Input;
using System.Numerics;

namespace MalignEngine;

using ISilkMouse = Silk.NET.Input.IMouse;
using ISilkKeyboard = Silk.NET.Input.IKeyboard;

public class Mouse : IMouse
{
    private readonly ISilkMouse _mouse;

    private Vector2 _lastPosition;
    private bool[] _prevButtons = new bool[8];

    public Mouse(ISilkMouse mouse)
    {
        _mouse = mouse;
        _lastPosition = mouse.Position;
    }

    public Vector2 Position { get; private set; }
    public Vector2 Delta { get; private set; }
    public float ScrollDelta { get; private set; }

    public bool IsButtonPressed(MouseButton button)
        => _mouse.IsButtonPressed((Silk.NET.Input.MouseButton)button);

    public bool WasButtonPressed(MouseButton button)
    {
        int index = (int)button;
        return !_prevButtons[index] && _mouse.IsButtonPressed((Silk.NET.Input.MouseButton)button);
    }

    public void Update()
    {
        Position = _mouse.Position;
        ScrollDelta = _mouse.ScrollWheels[0].Y;
        Delta = Position - _lastPosition;
        _lastPosition = _mouse.Position;

        for (int i = 0; i < _prevButtons.Length; i++)
        {
            _prevButtons[i] = _mouse.IsButtonPressed((Silk.NET.Input.MouseButton)i);
        }
    }
}

public class Keyboard : IKeyboard
{
    private readonly ISilkKeyboard _keyboard;
    private bool[] _prevKeys = new bool[512];

    public Keyboard(ISilkKeyboard keyboard)
    {
        _keyboard = keyboard;
    }

    public bool IsKeyPressed(Key key)
        => _keyboard.IsKeyPressed((Silk.NET.Input.Key)key);

    public bool WasKeyPressed(Key key)
    {
        int index = (int)key;
        return !_prevKeys[index] && _keyboard.IsKeyPressed((Silk.NET.Input.Key)key);
    }

    public void Update()
    {
        foreach (int key in Enum.GetValues(typeof(Silk.NET.Input.Key)))
        {
            if (key <= 0) { continue; }

            _prevKeys[key] = _keyboard.IsKeyPressed((Silk.NET.Input.Key)key);
        }
    }
}

public interface ISilkInputContextProvider
{
    IInputContext InputContext { get; }
}

[Stage<IUpdate, HighestPriorityStage>]
public class InputService : BaseSystem, IInputService, ISilkInputContextProvider
{
    public IInputContext InputContext => _inputContext;

    public IEnumerable<IMouse> Mices => _mice;
    public IEnumerable<IKeyboard> Keyboards => _keyboards;
    public IMouse Mouse => _mice.First();
    public IKeyboard Keyboard => Keyboards.First();

    private readonly IInputContext _inputContext;
    private readonly List<Mouse> _mice = new();
    private readonly List<Keyboard> _keyboards = new();

    public InputService(ILoggerService loggerService, IScheduleManager scheduleManager, WindowService windowService)
        : base(loggerService, scheduleManager)
    {
        _inputContext = windowService.window.CreateInput();

        foreach (var mouse in _inputContext.Mice)
        {
            _mice.Add(new Mouse(mouse));
        }

        foreach (var keyboard in _inputContext.Keyboards)
        {
            _keyboards.Add(new Keyboard(keyboard));
        }
    }

    public override void OnUpdate(float delta)
    {
        foreach (var mouse in _mice)
        {
            mouse.Update();
        }
        foreach (var keyboard in _keyboards)
        {
            keyboard.Update();
        }
    }

    public override void Dispose() => _inputContext.Dispose();
}
