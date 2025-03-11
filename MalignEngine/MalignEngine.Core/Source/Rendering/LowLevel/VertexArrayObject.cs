using Silk.NET.Core.Attributes;
using Silk.NET.OpenGL;
using System;

namespace MalignEngine
{
    public enum VertexAttributeType
    {
        Byte, UnsignedByte, Short, UnsignedShort, Int, UnsignedInt, Float, Double
    }

    public abstract class VertexArrayObject : IDisposable
    {
        public VertexArrayObject() { }

        public abstract void PushVertexAttribute(int count, VertexAttributeType type);
        public abstract void Dispose();
    }
}
