using nkast.Wasm.Canvas.WebGL;

namespace MalignEngine;

public class WebBufferResource : IBufferResource
{
    private IWebGL2RenderingContext _gl;
    private IRenderingAPI _renderAPI;
    private IBufferResourceDescriptor _descriptor;

    private WebGLBuffer _buffer;
    private WebGLBufferType _bufferType;
    private WebGLBufferUsageHint _usageType;

    public WebBufferResource(IWebGL2RenderingContext gl, IRenderingAPI renderAPI, IBufferResourceDescriptor descriptor)
    {
        _gl = gl;
        _descriptor = descriptor;
        _renderAPI = renderAPI;

        switch (_descriptor.BufferObjectType)
        {
            case BufferObjectType.Element:
                _bufferType = WebGLBufferType.ELEMENT_ARRAY;
                break;
            case BufferObjectType.Vertex:
                _bufferType = WebGLBufferType.ARRAY;
                break;
        }

        switch (_descriptor.BufferUsageType)
        {
            case BufferUsageType.Static:
                _usageType = WebGLBufferUsageHint.STATIC_DRAW;
                break;
            case BufferUsageType.Dynamic:
                _usageType = WebGLBufferUsageHint.DYNAMIC_DRAW;
                break;
            case BufferUsageType.Stream:
                _usageType = WebGLBufferUsageHint.STREAM_DRAW;
                break;
        }

        _renderAPI.Submit(ctx =>
        {
            _buffer = _gl.CreateBuffer();
            Bind();
            if (descriptor.InitialData != null)
            {
                gl.BufferData<byte>(_bufferType, descriptor.InitialData, _usageType);
            }
        });
    }

    public void BufferData<TDataType>(TDataType[] data) where TDataType : unmanaged
    {
        _renderAPI.Submit(ctx =>
        {
            Bind();
            _gl.BufferData<TDataType>(_bufferType, data, _usageType);
        });
    }

    public void BufferData<TDataType>(TDataType[] data, int offset, uint length) where TDataType : unmanaged
    {
        _renderAPI.Submit(ctx =>
        {
            Bind();
            unsafe
            {
                fixed (void* d = data)
                {
                    _gl.BufferSubData<TDataType>(_bufferType, offset, data, (int)length);
                }
            }
        });
    }

    public void Bind()
    {
        _gl.BindBuffer(_bufferType, _buffer);
    }

    public void Dispose()
    {
        _renderAPI.Submit(ctx =>
        {
            _buffer.Dispose();
        });
    }
}