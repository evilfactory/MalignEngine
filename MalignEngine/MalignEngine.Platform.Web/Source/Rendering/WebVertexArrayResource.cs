using nkast.Wasm.Canvas.WebGL;

namespace MalignEngine;

public class WebVertexArrayResource : IVertexArrayResource
{
    private IWebGL2RenderingContext _gl;
    private IVertexArrayDescriptor _descriptor;
    private IRenderingAPI _renderAPI;

    private uint _handle;

    public WebVertexArrayResource(IWebGL2RenderingContext gl, IRenderingAPI renderAPI, IVertexArrayDescriptor descriptor)
    {
        _gl = gl;
        _descriptor = descriptor;
        _renderAPI = renderAPI;

        _renderAPI.Submit(() =>
        {
        });
    }

    public void Bind()
    {
        foreach (var attribute in _descriptor.Attributes)
        {
            _gl.EnableVertexAttribArray(attribute.Location);

            _gl.VertexAttribPointer(
                attribute.Location,
                attribute.ComponentCount,
                GetPointerType(attribute.Type),
                false,
                _descriptor.Stride,
                attribute.Offset);
        }
    }

    public void Dispose()
    {
        _renderAPI.Submit(ctx =>
        {
        });
    }

    private static WebGLDataType GetPointerType(VertexAttributeType type)
    {
        switch (type)
        {
            case VertexAttributeType.Byte:
                return WebGLDataType.BYTE;
            case VertexAttributeType.UnsignedByte:
                return WebGLDataType.UBYTE;
            case VertexAttributeType.Short:
                return WebGLDataType.SHORT;
            case VertexAttributeType.UnsignedShort:
                return WebGLDataType.USHORT;
            case VertexAttributeType.Int:
                return WebGLDataType.INT;
            case VertexAttributeType.UnsignedInt:
                return WebGLDataType.UINT;
            case VertexAttributeType.Float:
                return WebGLDataType.FLOAT;
            case VertexAttributeType.Double:
                throw new NotImplementedException("Not implemented");
            default:
                return WebGLDataType.BYTE;
        }
    }
}
