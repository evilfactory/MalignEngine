using Silk.NET.OpenGL;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace MalignEngine
{
    public class GLVertexArrayObject : VertexArrayObject, IDisposable
    {
        private class VertexAttribute
        {
            public int count;
            public VertexAttributeType type;
        }

        private uint _handle;
        private GL gl;

        private bool built = false;

        private List<VertexAttribute> attributes = new List<VertexAttribute>();

        public GLVertexArrayObject(GL gl)
        {
            this.gl = gl;

            _handle = this.gl.GenVertexArray();
        }

        public override void PushVertexAttribute(int count, VertexAttributeType type)
        {
            attributes.Add(new VertexAttribute { count = count, type = type });
        }

        public void Bind()
        {
            gl.BindVertexArray(_handle);

            if (!built)
            {
                unsafe
                {
                    int stride = 0;
                    for (uint index = 0; index < attributes.Count; index++)
                    {
                        stride += attributes[(int)index].count * GetSize(attributes[(int)index].type);
                    }

                    int offset = 0;
                    for (uint index = 0; index < attributes.Count; index++)
                    {
                        var type = attributes[(int)index].type;
                        var count = attributes[(int)index].count;

                        gl.VertexAttribPointer(index, count, (GLEnum)GetPointerType(type), false, (uint)stride, (void*)offset);
                        gl.EnableVertexAttribArray(index);

                        offset += count * GetSize(type);
                    }
                }

                built = true;
            }
        }

        public override void Dispose()
        {
            gl.DeleteVertexArray(_handle);
        }

        private static int GetSize(VertexAttributeType type)
        {
            switch (type)
            {
                case VertexAttributeType.Byte:
                    return sizeof(sbyte);
                case VertexAttributeType.UnsignedByte:
                    return sizeof(byte);
                case VertexAttributeType.Short:
                    return sizeof(short);
                case VertexAttributeType.UnsignedShort:
                    return sizeof(ushort);
                case VertexAttributeType.Int:
                    return sizeof(int);
                case VertexAttributeType.UnsignedInt:
                    return sizeof(uint);
                case VertexAttributeType.Float:
                    return sizeof(float);
                case VertexAttributeType.Double:
                    return sizeof(double);
                default:
                    return 0;
            }
        }

        private static VertexAttribPointerType GetPointerType(VertexAttributeType type)
        {
            switch (type)
            {
                case VertexAttributeType.Byte:
                    return VertexAttribPointerType.Byte;
                case VertexAttributeType.UnsignedByte:
                    return VertexAttribPointerType.UnsignedByte;
                case VertexAttributeType.Short:
                    return VertexAttribPointerType.Short;
                case VertexAttributeType.UnsignedShort:
                    return VertexAttribPointerType.UnsignedShort;
                case VertexAttributeType.Int:
                    return VertexAttribPointerType.Int;
                case VertexAttributeType.UnsignedInt:
                    return VertexAttribPointerType.UnsignedInt;
                case VertexAttributeType.Float:
                    return VertexAttribPointerType.Float;
                case VertexAttributeType.Double:
                    return VertexAttribPointerType.Double;
                default:
                    return VertexAttribPointerType.Byte;
            }
        }
    }
}
