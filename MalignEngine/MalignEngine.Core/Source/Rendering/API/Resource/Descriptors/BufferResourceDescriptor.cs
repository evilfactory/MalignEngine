using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public interface IBufferResourceDescriptor
{
    BufferObjectType BufferObjectType { get; }
    BufferUsageType BufferUsageType { get; }
    byte[]? InitialData { get; }
}

public class BufferResourceDescriptor : IBufferResourceDescriptor
{
    public BufferObjectType BufferObjectType { get; }
    public BufferUsageType BufferUsageType { get; }
    public byte[]? InitialData { get; }

    public BufferResourceDescriptor(BufferObjectType objectType, BufferUsageType usageType, byte[]? initialData = null)
    {
        BufferObjectType = objectType;
        BufferUsageType = usageType;
        InitialData = initialData;
    }
}
