using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public enum VertexAttributeType
{
    Byte, UnsignedByte, Short, UnsignedShort, Int, UnsignedInt, Float, Double
}

public interface IVertexAttributeDescriptor
{
    string Name { get; }
    int Location { get; }
    VertexAttributeType Type { get; }
    int ComponentCount { get; }
    bool Normalized { get; }
    int Offset { get; }
}

public interface IVertexArrayDescriptor
{
    IReadOnlyList<IVertexAttributeDescriptor> Attributes { get; }
    int Stride { get; }

    void AddAttribute(string name, int location, VertexAttributeType type, int componentCount, bool normalized = false);
}

public class VertexAttributeDescriptor : IVertexAttributeDescriptor
{
    public string Name { get; }
    public int Location { get; }
    public VertexAttributeType Type { get; }
    public int ComponentCount { get; }
    public bool Normalized { get; }
    public int Offset { get; internal set; }

    public VertexAttributeDescriptor(string name, int location, VertexAttributeType type, int componentCount, bool normalized)
    {
        Name = name;
        Location = location;
        Type = type;
        ComponentCount = componentCount;
        Normalized = normalized;
    }
}

public class VertexArrayDescriptor : IVertexArrayDescriptor
{
    private readonly List<VertexAttributeDescriptor> _attributes = new();

    public IReadOnlyList<IVertexAttributeDescriptor> Attributes => _attributes;

    public int Stride { get; private set; } = 0;

    public void AddAttribute(string name, int location, VertexAttributeType type, int componentCount, bool normalized = false)
    {
        int size = GetTypeSize(type) * componentCount;
        var attr = new VertexAttributeDescriptor(name, location, type, componentCount, normalized)
        {
            Offset = Stride
        };

        _attributes.Add(attr);
        Stride += size;
    }

    private static int GetTypeSize(VertexAttributeType type)
    {
        return type switch
        {
            VertexAttributeType.Float => 4,
            VertexAttributeType.Int => 4,
            VertexAttributeType.UnsignedInt => 4,
            VertexAttributeType.Byte => 1,
            VertexAttributeType.UnsignedByte => 1,
            VertexAttributeType.Short => 2,
            VertexAttributeType.UnsignedShort => 2,
            VertexAttributeType.Double => 8,
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Unknown vertex attribute type")
        };
    }
}