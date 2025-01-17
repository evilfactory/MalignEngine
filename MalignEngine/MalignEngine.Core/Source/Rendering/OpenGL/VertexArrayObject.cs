using Silk.NET.OpenGL;
using System;

namespace MalignEngine
{
    public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        private uint _handle;
        private GL gl;

        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
        {
            this.gl = gl;

            _handle = this.gl.GenVertexArray();
            Bind();
            vbo.Bind();
            ebo.Bind();
        }

        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
        {
            gl.VertexAttribPointer(index, count, type, false, vertexSize, (void*)offSet);
            gl.EnableVertexAttribArray(index);
        }

        public void Bind()
        {
            gl.BindVertexArray(_handle);
        }

        public void Dispose()
        {
            gl.DeleteVertexArray(_handle);
        }
    }
}
