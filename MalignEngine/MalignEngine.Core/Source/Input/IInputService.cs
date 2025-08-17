using System.Numerics;

namespace MalignEngine;

public interface IMouse
{
    Vector2 Position { get; }
    Vector2 Delta { get; }
    float ScrollDelta { get; }
    bool IsButtonPressed(MouseButton button);
    bool WasButtonPressed(MouseButton button);
}

public interface IKeyboard
{
    bool IsKeyPressed(Key key);
    bool WasKeyPressed(Key key);
}

public interface IInputService : IService
{
    IMouse Mouse { get; }
    IKeyboard Keyboard { get; }

    IEnumerable<IMouse> Mices { get; }
    IEnumerable<IKeyboard> Keyboards { get; }
}