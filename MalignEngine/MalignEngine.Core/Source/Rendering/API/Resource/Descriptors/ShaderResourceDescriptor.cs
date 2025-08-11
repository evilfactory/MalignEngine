using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public interface IShaderResourceDescriptor
{
    string VertexShaderSource { get; }
    string FragmentShaderSource { get; }

    string? DebugName { get; }
}

public class ShaderResourceDescriptor : IShaderResourceDescriptor
{
    public string VertexShaderSource { get; set; } = string.Empty;
    public string FragmentShaderSource { get; set; } = string.Empty;
    public string? DebugName { get; set; }

    public ShaderResourceDescriptor() { }

    public ShaderResourceDescriptor(string vertexSource, string fragmentSource, string? debugName = null)
    {
        VertexShaderSource = vertexSource;
        FragmentShaderSource = fragmentSource;
        DebugName = debugName;
    }
}
