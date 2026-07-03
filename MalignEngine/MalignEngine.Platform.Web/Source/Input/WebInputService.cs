using nkast.Wasm.Dom;
using System.Numerics;

namespace MalignEngine;

public class WebMouse : IMouse
{
    public Vector2 Position { get; private set; }
    public Vector2 Delta { get; private set; }
    public float ScrollDelta { get; private set; }

    private Dictionary<MouseButton, bool> _prevButtons = [];
    private Dictionary<MouseButton, bool> _buttons = [];

    private Vector2 _lastPosition;

    public WebMouse(Window window)
    {
        window.OnMouseDown += OnMouseDown;
        window.OnMouseUp += OnMouseUp;
        window.OnMouseMove += OnMouseMove;
        window.OnMouseWheel += OnMouseWheel;

        foreach (var m in Enum.GetValues<MouseButton>())
        {
            _prevButtons[m] = false;
            _buttons[m] = false;
        }
    }

    public bool IsButtonPressed(MouseButton button)
    {
        return _buttons.TryGetValue(button, out bool pressed) && pressed;
    }

    public bool WasButtonPressed(MouseButton button)
    {
        return _buttons[button] && !_prevButtons[button];
    }

    public bool WasButtonReleased(MouseButton button)
    {
        return !_buttons[button] && _prevButtons[button];
    }

    public void Update()
    {
        Delta = Position - _lastPosition;
        _lastPosition = Position;

        ScrollDelta = 0;

        foreach (MouseButton button in Enum.GetValues<MouseButton>())
        {
            _prevButtons[button] = _buttons[button];
        }
    }

    private void OnMouseDown(object sender, int x, int y, int button)
    {
        Position = new Vector2(x, y);

        // insanity
        button--;
        if (Enum.IsDefined(typeof(MouseButton), button))
        {
            _buttons[(MouseButton)button] = true;
        }
    }

    private void OnMouseUp(object sender, int x, int y, int button)
    {
        Position = new Vector2(x, y);

        if (Enum.IsDefined(typeof(MouseButton), button))
        {
            _buttons[(MouseButton)button] = false;
        }
    }

    private void OnMouseMove(object sender, int x, int y)
    {
        Position = new Vector2(x, y);
    }

    private void OnMouseWheel(object sender, int deltaX, int deltaY, int deltaZ, int deltaMode)
    {
        ScrollDelta += deltaY;
    }
}

public class WebKeyboard : IKeyboard
{
    public static Key TranslateKey(int keyCode, int location)
    {
        return keyCode switch
        {
            // Letters
            >= 65 and <= 90 => (Key)keyCode,

            // Numbers
            >= 48 and <= 57 => (Key)keyCode,

            // Punctuation
            32 => Key.Space,
            186 => Key.Semicolon,
            187 => Key.Equals,
            188 => Key.Comma,
            189 => Key.Minus,
            190 => Key.Period,
            191 => Key.Slash,
            192 => Key.GraveAccent,
            219 => Key.LeftBracket,
            220 => Key.BackSlash,
            221 => Key.RightBracket,
            222 => Key.Apostrophe,

            // Navigation
            27 => Key.Esc,
            13 => location == 3 ? Key.NumpadEnter : Key.Enter,
            9 => Key.Tab,
            8 => Key.Backspace,
            45 => Key.Insert,
            46 => Key.Delete,
            36 => Key.Home,
            35 => Key.End,
            33 => Key.PageUp,
            34 => Key.PageDown,

            // Arrow keys
            37 => Key.LeftArrow,
            38 => Key.UpArrow,
            39 => Key.RightArrow,
            40 => Key.DownArrow,

            // Locks
            20 => Key.CapsLock,
            144 => Key.NumLock,
            145 => Key.ScrollLock,
            44 => Key.PrintScreen,
            19 => Key.Pause,

            // Function keys
            >= 112 and <= 123 => (Key)(Key.F1 + (keyCode - 112)),

            // Modifiers
            16 => location == 2 ? Key.RightShift : Key.LeftShift,
            17 => location == 2 ? Key.RightCtrl : Key.LeftCtrl,
            18 => location == 2 ? Key.RightAlt : Key.LeftAlt,
            93 => Key.Menu,

            // Numpad
            96 => Key.Numpad0,
            97 => Key.Numpad1,
            98 => Key.Numpad2,
            99 => Key.Numpad3,
            100 => Key.Numpad4,
            101 => Key.Numpad5,
            102 => Key.Numpad6,
            103 => Key.Numpad7,
            104 => Key.Numpad8,
            105 => Key.Numpad9,
            106 => Key.NumpadMultiply,
            107 => Key.NumpadAdd,
            109 => Key.NumpadSubtract,
            110 => Key.NumpadDecimal,
            111 => Key.NumpadDivide,

            _ => Key.Unknown
        };
    }

    private Dictionary<Key, bool> _keys = [];
    private Dictionary<Key, bool> _prevKeys = [];

    public WebKeyboard(Window window)
    {
        foreach (var m in Enum.GetValues<Key>())
        {
            _keys[m] = false;
            _prevKeys[m] = false;
        }

        window.OnKeyDown += OnKeyDown;
        window.OnKeyUp += OnKeyUp;
    }

    public bool IsKeyPressed(Key key)
    {
        if (key == Key.Any)
        {
            return _keys.Values.Any(v => v);
        }

        return _keys.TryGetValue(key, out bool pressed) && pressed;
    }

    public bool WasKeyPressed(Key key)
    {
        if (key == Key.Any)
        {
            return _keys.Any(kvp => kvp.Value && !_prevKeys[kvp.Key]);
        }

        return _keys[key] && !_prevKeys[key];
    }

    public bool WasKeyReleased(Key key)
    {
        if (key == Key.Any)
        {
            return _keys.Any(kvp => !kvp.Value && _prevKeys[kvp.Key]);
        }

        return !_keys[key] && _prevKeys[key];
    }

    public void Update()
    {
        foreach (Key key in Enum.GetValues<Key>())
        {
            _prevKeys[key] = _keys[key];
        }
    }

    private void OnKeyDown(object sender, char key, int keyCode, int location)
    {
        Key translated = TranslateKey(keyCode, location);

        if (translated != Key.Unknown)
        {
            _keys[translated] = true;
        }
    }

    private void OnKeyUp(object sender, char key, int keyCode, int location)
    {
        Key translated = TranslateKey(keyCode, location);

        if (translated != Key.Unknown)
        {
            _keys[translated] = false;
        }
    }
}


[Stage<IUpdate, HighestPriorityStage>]
public class WebInputService : BaseSystem, IInputService
{
    public IEnumerable<IMouse> Mices => _mice;
    public IEnumerable<IKeyboard> Keyboards => _keyboards;
    public IMouse Mouse => _mice.First();
    public IKeyboard Keyboard => Keyboards.First();

    private readonly List<WebMouse> _mice = new();
    private readonly List<WebKeyboard> _keyboards = new();

    public WebInputService(IServiceContainer serviceContainer)
        : base(serviceContainer)
    {
        _mice.Add(new WebMouse(Window.Current));
        _keyboards.Add(new WebKeyboard(Window.Current));
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

    public override void Dispose() { }
}
