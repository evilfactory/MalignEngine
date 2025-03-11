using Silk.NET.OpenGL;
using System;

namespace MalignEngine
{
    public class GLBufferObject<TDataType> : BufferObject<TDataType>
        where TDataType : unmanaged
    {
        private uint handle;
        private BufferTargetARB bufferType;
        private BufferUsageARB usageType;
        private GL gl;

        public GLBufferObject(GL gl, Span<TDataType> data, BufferObjectType type, BufferUsageType usage) : base(type, usage, data)
        {
            this.gl = gl;

            switch (type)
            {
                case BufferObjectType.Element:
                    bufferType = BufferTargetARB.ElementArrayBuffer;
                    break;
                case BufferObjectType.Vertex:
                    bufferType = BufferTargetARB.ArrayBuffer;
                    break;
            }

            switch (usage)
            {
                case BufferUsageType.Static:
                    usageType = BufferUsageARB.StaticDraw;
                    break;
                case BufferUsageType.Dynamic:
                    usageType = BufferUsageARB.DynamicDraw;
                    break;
                case BufferUsageType.Stream:
                    usageType = BufferUsageARB.StreamDraw;
                    break;
            }


            handle = this.gl.GenBuffer();
            Bind();
            unsafe
            {
                fixed (void* d = data)
                {
                    gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, usageType);
                }
            }
        }

        public override void BufferData(Span<TDataType> data)
        {
            Bind();
            unsafe
            {
                fixed (void* d = data)
                {
                    gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, usageType);
                }
            }
        }

        public void Bind()
        {
            gl.BindBuffer(bufferType, handle);
        }

        public override void Dispose()
        {
            gl.DeleteBuffer(handle);
        }
    }
}
