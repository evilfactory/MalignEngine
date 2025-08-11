using Silk.NET.OpenGL;

namespace MalignEngine;

public class GLBufferResource : IBufferResource
{
    private GL _gl;
    private GLRenderingAPI _renderAPI;
    private IBufferResourceDescriptor _descriptor;

    private uint _handle;
    private BufferTargetARB _bufferType;
    private BufferUsageARB _usageType;

    public GLBufferResource(GL gl, GLRenderingAPI renderAPI, IBufferResourceDescriptor descriptor)
    {
        _gl = gl;
        _descriptor = descriptor;
        _renderAPI = renderAPI;

        switch (_descriptor.BufferObjectType)
        {
            case BufferObjectType.Element:
                _bufferType = BufferTargetARB.ElementArrayBuffer;
                break;
            case BufferObjectType.Vertex:
                _bufferType = BufferTargetARB.ArrayBuffer;
                break;
        }

        switch (_descriptor.BufferUsageType)
        {
            case BufferUsageType.Static:
                _usageType = BufferUsageARB.StaticDraw;
                break;
            case BufferUsageType.Dynamic:
                _usageType = BufferUsageARB.DynamicDraw;
                break;
            case BufferUsageType.Stream:
                _usageType = BufferUsageARB.StreamDraw;
                break;
        }

        _renderAPI.Submit(ctx =>
        {
            _handle = _gl.GenBuffer();
            Bind();
            unsafe
            {
                if (descriptor.InitialData != null)
                {
                    fixed (void* d = descriptor.InitialData)
                    {
                        gl.BufferData(_bufferType, (nuint)(descriptor.InitialData.Length * sizeof(byte)), d, _usageType);
                    }
                }
            }
        });
    }

    public void BufferData<TDataType>(byte[] data) where TDataType : unmanaged
    {
        _renderAPI.Submit(ctx =>
        {
            Bind();
            unsafe
            {
                fixed (void* d = data)
                {
                    _gl.BufferData(_bufferType, (nuint)(data.Length * sizeof(TDataType)), d, _usageType);
                }
            }
        });
    }

    public void BufferData<TDataType>(byte[] data, int offset, uint length) where TDataType : unmanaged
    {
        _renderAPI.Submit(ctx =>
        {
            Bind();
            unsafe
            {
                fixed (void* d = data)
                {
                    _gl.BufferSubData((GLEnum)_bufferType, offset, (nuint)(length * sizeof(TDataType)), d);
                }
            }
        });
    }

    public void Bind()
    {
        _gl.BindBuffer(_bufferType, _handle);
    }

    public void Dispose()
    {
        _renderAPI.Submit(ctx =>
        {
            _gl.DeleteBuffer(_handle);
        });
    }
}