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

public interface IBufferResource : IGpuResource
{
    void BufferData<TDataType>(byte[] data) where TDataType : unmanaged;
    void BufferData<TDataType>(byte[] data, int offset, uint length) where TDataType : unmanaged;
}