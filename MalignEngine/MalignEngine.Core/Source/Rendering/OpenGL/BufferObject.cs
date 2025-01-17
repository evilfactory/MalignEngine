using Silk.NET.OpenGL;
using System;

namespace MalignEngine
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        private uint handle;
        private BufferTargetARB bufferType;
        private GL gl;

        public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
        {
            this.gl = gl;
            this.bufferType = bufferType;

            handle = this.gl.GenBuffer();
            Bind();
            fixed (void* d = data)
            {
                gl.BufferData(bufferType, (nuint) (data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
            }
        }

        public void Bind()
        {
            gl.BindBuffer(bufferType, handle);
        }

        public void Dispose()
        {
            gl.DeleteBuffer(handle);
        }
    }
}
