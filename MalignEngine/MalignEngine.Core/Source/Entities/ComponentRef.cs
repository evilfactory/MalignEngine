namespace MalignEngine;

public readonly ref struct ComponentRef<T> where T : IComponent
{
    private readonly ref T _value;

    public ComponentRef(ref T value)
    {
        _value = ref value;
    }

    public ref T Value => ref _value;
}