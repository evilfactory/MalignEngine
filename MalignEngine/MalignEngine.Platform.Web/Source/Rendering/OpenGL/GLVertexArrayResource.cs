using Silk.NET.OpenGL;

namespace MalignEngine;

public class GLVertexArrayResource : IVertexArrayResource
{
    private GL _gl;
    private IVertexArrayDescriptor _descriptor;
    private GLRenderingAPI _renderAPI;

    private uint _handle;

    public GLVertexArrayResource(GL gl, GLRenderingAPI renderAPI, IVertexArrayDescriptor descriptor)
    {
        _gl = gl;
        _descriptor = descriptor;
        _renderAPI = renderAPI;

        _renderAPI.Submit(() =>
        {
            _handle = _gl.GenVertexArray();
        });
    }

    public void Bind()
    {
        _gl.BindVertexArray(_handle);

        foreach (IVertexAttributeDescriptor attribute in _descriptor.Attributes)
        {
            unsafe
            {
                _gl.VertexAttribPointer((uint)attribute.Location, attribute.ComponentCount, (GLEnum)GetPointerType(attribute.Type), false, (uint)_descriptor.Stride, (void*)attribute.Offset);
                _gl.EnableVertexAttribArray((uint)attribute.Location);
            }
        }
    }

    public void Dispose()
    {
        _renderAPI.Submit(ctx =>
        {
            _gl.DeleteVertexArray(_handle);
        });
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
