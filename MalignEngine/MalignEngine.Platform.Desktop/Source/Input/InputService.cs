using Silk.NET.Input;
using System.Numerics;

namespace MalignEngine;

using ISilkMouse = Silk.NET.Input.IMouse;
using ISilkKeyboard = Silk.NET.Input.IKeyboard;

public class Mouse : IMouse
{
    private readonly ISilkMouse _mouse;

    private Vector2 _lastPosition;
    private readonly bool[] _buttons = new bool[8];
    private readonly bool[] _prevButtons = new bool[8];

    public Mouse(ISilkMouse mouse)
    {
        _mouse = mouse;
        _lastPosition = mouse.Position;
    }

    public Vector2 Position { get; private set; }
    public Vector2 Delta { get; private set; }
    public float ScrollDelta { get; private set; }

    public bool IsDown(MouseButton button)
    {
        return _buttons[(int)button];
    }

    public bool IsPressed(MouseButton button)
    {
        int i = (int)button;
        return _buttons[i] && !_prevButtons[i];
    }

    public bool IsReleased(MouseButton button)
    {
        int i = (int)button;
        return !_buttons[i] && _prevButtons[i];
    }

    public void Update()
    {
        Position = _mouse.Position;
        Delta = Position - _lastPosition;
        _lastPosition = Position;

        ScrollDelta = _mouse.ScrollWheels[0].Y;

        Array.Copy(_buttons, _prevButtons, _buttons.Length);

        for (int i = 0; i < _buttons.Length; i++)
        {
            _buttons[i] = _mouse.IsButtonPressed((Silk.NET.Input.MouseButton)i);
        }
    }
}

public class Keyboard : IKeyboard
{
    private readonly ISilkKeyboard _keyboard;

    private readonly bool[] _currentKeys = new bool[512];
    private readonly bool[] _previousKeys = new bool[512];

    public Action<char>? OnTextInput { get; set; }
    public Action<Key>? OnKeyPressed { get; set; }
    public Action<Key>? OnKeyReleased { get; set; }

    public Keyboard(ISilkKeyboard keyboard)
    {
        _keyboard = keyboard;

        _keyboard.KeyChar += Keyboard_KeyChar;
        _keyboard.KeyDown += _keyboard_KeyDown;
        _keyboard.KeyUp += _keyboard_KeyUp;
    }

    private void _keyboard_KeyUp(ISilkKeyboard arg1, Silk.NET.Input.Key arg2, int arg3)
    {
        OnKeyReleased?.Invoke((Key)arg2);
    }

    private void _keyboard_KeyDown(ISilkKeyboard arg1, Silk.NET.Input.Key arg2, int arg3)
    {
        OnKeyPressed?.Invoke((Key)arg2);
    }

    private void Keyboard_KeyChar(ISilkKeyboard arg1, char arg2)
    {
        OnTextInput?.Invoke(arg2);
    }

    public bool IsDown(Key key)
    { 
        return _currentKeys[(int)key];
    }

    public bool IsPressed(Key key)
    {
        int i = (int)key;
        return _currentKeys[i] && !_previousKeys[i];
    }

    public bool IsReleased(Key key)
    {
        int i = (int)key;
        return !_currentKeys[i] && _previousKeys[i];
    }

    public void Update()
    {
        Array.Copy(_currentKeys, _previousKeys, _currentKeys.Length);

        foreach (var key in Enum.GetValues<Silk.NET.Input.Key>())
        {
            if (key == Silk.NET.Input.Key.Unknown) { continue; }

            _currentKeys[(int)key] = _keyboard.IsKeyPressed(key);
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

    public InputService(IServiceContainer serviceContainer, WindowService windowService)
        : base(serviceContainer)
    {
        _inputContext = windowService.window.CreateInput();

        foreach (var mouse in _inputContext.Mice)
        {
            _mice.Add(new Mouse(mouse));
        }

        foreach (var silkKeyboard in _inputContext.Keyboards)
        {
            Keyboard keyboard = new Keyboard(silkKeyboard);

            _keyboards.Add(keyboard);

            keyboard.OnTextInput = (char c) =>
            {
                ScheduleManager.Run<ITextInput>(x => x.OnTextInput(c));
            };

            keyboard.OnKeyPressed = (Key key) =>
            {
                ScheduleManager.Run<IKeyPressed>(x => x.OnKeyPressed(key));
            };

            keyboard.OnKeyReleased = (Key key) =>
            {
                ScheduleManager.Run<IKeyReleased>(x => x.OnKeyReleased(key));
            };
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
