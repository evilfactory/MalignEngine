using System.Numerics;

namespace MalignEngine;

public interface ITextInput : ISchedule
{
    void OnTextInput(char input);
}

public interface IKeyPressed : ISchedule
{
    void OnKeyPressed(Key key);
}

public interface IKeyReleased : ISchedule
{
    void OnKeyReleased(Key key);
}


public interface IMouse
{
    Vector2 Position { get; }
    Vector2 Delta { get; }
    float ScrollDelta { get; }
    bool IsDown(MouseButton button);
    bool IsPressed(MouseButton button);
    bool IsReleased(MouseButton button);
}

public interface IKeyboard
{
    bool IsDown(Key key);
    bool IsPressed(Key key);
    bool IsReleased(Key key);
}

public interface IInputService : IService
{
    IMouse Mouse { get; }
    IKeyboard Keyboard { get; }

    IEnumerable<IMouse> Mices { get; }
    IEnumerable<IKeyboard> Keyboards { get; }
}