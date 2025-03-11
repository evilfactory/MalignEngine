using Silk.NET.OpenGL;
using System;
using System.Reflection;

namespace MalignEngine
{
    public class GLVertexArrayObject : VertexArrayObject, IDisposable
    {
        private uint _handle;
        private GL gl;

        private uint index = 0;
        private int offset = 0;

        public GLVertexArrayObject(GL gl)
        {
            this.gl = gl;

            _handle = this.gl.GenVertexArray();
        }

        public override void PushVertexAttribute(int count, VertexAttributeType type)
        {
            Bind();

            unsafe
            {
                switch (type)
                {
                    case VertexAttributeType.Byte:
                        gl.VertexAttribPointer(index, count, VertexAttribPointerType.Byte, false, 0, (void*)offset);
                        offset += count * sizeof(byte);
                        break;
                    case VertexAttributeType.UnsignedByte:
                        gl.VertexAttribPointer(index, count, VertexAttribPointerType.UnsignedByte, false, 0, (void*)offset);
                        offset += count * sizeof(sbyte);
                        break;
                    case VertexAttributeType.Short:
                        gl.VertexAttribPointer(index, count, VertexAttribPointerType.Short, false, 0, (void*)offset);
                        offset += count * sizeof(short);
                        break;
                    case VertexAttributeType.UnsignedShort:
                        gl.VertexAttribPointer(index, count, VertexAttribPointerType.UnsignedByte, false, 0, (void*)offset);
                        offset += count * sizeof(ushort);
                        break;
                    case VertexAttributeType.Int:
                        gl.VertexAttribPointer(index, count, VertexAttribPointerType.Int, false, 0, (void*)offset);
                        offset += count * sizeof(int);
                        break;
                    case VertexAttributeType.UnsignedInt:
                        gl.VertexAttribPointer(index, count, VertexAttribPointerType.UnsignedInt, false, 0, (void*)offset);
                        offset += count * sizeof(uint);
                        break;
                    case VertexAttributeType.Float:
                        gl.VertexAttribPointer(index, count, VertexAttribPointerType.Float, false, 0, (void*)offset);
                        offset += count * sizeof(float);
                        break;
                    case VertexAttributeType.Double:
                        gl.VertexAttribPointer(index, count, VertexAttribPointerType.Double, false, 0, (void*)offset);
                        offset += count * sizeof(double);
                        break;
                }
            }

            gl.EnableVertexAttribArray(index);

            index++;
        }

        public void Bind()
        {
            gl.BindVertexArray(_handle);
        }

        public override void Dispose()
        {
            gl.DeleteVertexArray(_handle);
        }
    }
}
