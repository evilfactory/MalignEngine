namespace MalignEngine;

public enum BufferObjectType
{
    Element,
    Vertex
}

public enum BufferUsageType
{
    Static,
    Dynamic,
    Stream
}

public abstract class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    public BufferObject(BufferObjectType type, BufferUsageType usageType, Span<TDataType> data)
    {

    }

    public abstract void BufferData(Span<TDataType> data);
    public abstract void BufferData(Span<TDataType> data, int offset, uint length);
    public abstract void Dispose();
}