using nkast.Wasm.Canvas.WebGL;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace MalignEngine;

public class WebShaderResource : IShaderResource
{
    private IWebGL2RenderingContext _gl;
    private IShaderResourceDescriptor _descriptor;
    private IRenderingAPI _renderAPI;

    private WebGLProgram _program;

    public WebShaderResource(IWebGL2RenderingContext gl, IRenderingAPI renderAPI, IShaderResourceDescriptor descriptor)
    {
        _gl = gl;
        _descriptor = descriptor;
        _renderAPI = renderAPI;

        renderAPI.Submit(ctx =>
        {
            StringBuilder vertexSource = new StringBuilder();
            vertexSource.AppendLine("#version 300 es");
            vertexSource.AppendLine("precision mediump float;");
            vertexSource.AppendLine(_descriptor.VertexShaderSource);

            StringBuilder fragmentSource = new StringBuilder();
            fragmentSource.AppendLine("#version 300 es");
            fragmentSource.AppendLine("precision mediump float;");
            fragmentSource.AppendLine(_descriptor.FragmentShaderSource);

            var vertex = LoadShader(WebGLShaderType.VERTEX, vertexSource.ToString());
            var fragment = LoadShader(WebGLShaderType.FRAGMENT, fragmentSource.ToString());
            _program = _gl.CreateProgram();
            _gl.AttachShader(_program, vertex);
            _gl.AttachShader(_program, fragment);
            _gl.LinkProgram(_program);
            /*
            _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");
            }
            _gl.DetachShader(_handle, vertex);
            _gl.DetachShader(_handle, fragment);
            _gl.DeleteShader(vertex);
            _gl.DeleteShader(fragment);
            */
        });
    }

    private WebGLShader LoadShader(WebGLShaderType type, string src)
    {
        WebGLShader shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, src);
        _gl.CompileShader(shader);
        string infoLog = _gl.GetShaderInfoLog(shader);

        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return shader;
    }

    public void Dispose()
    {

    }

    public void Use()
    {
        _gl.UseProgram(_program);
    }

    public bool HasUniform(string name)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        WebGLUniformLocation location = _gl.GetUniformLocation(_program, name);
        return location != null;
    }

    public void Set(string name, int value)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        _gl.Uniform1i(GetLocation(name), value);
    }

    public void Set(string name, uint value)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        _gl.Uniform1f(GetLocation(name), value);
    }

    public void Set(string name, Matrix4x4 value)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        Matrix4x4 copy = value;

        unsafe
        {
            _gl.UniformMatrix4fv(GetLocation(name), [value]);
        }
    }

    public void Set(string name, float value)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        _gl.Uniform1f(GetLocation(name), value);
    }

    public void Set(string name, int[] value)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        _gl.Uniform1iv(GetLocation(name), value);
    }

    public void Set(string name, Color color)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        _gl.Uniform4f(GetLocation(name), color.R, color.G, color.B, color.A);
    }

    public void Set(string name, Vector2[] value)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        float[] floatArray = new float[value.Length * 2];
        int index = 0;
        for (int i = 0; i < value.Length; i++)
        {
            floatArray[index] = value[i].X;
            floatArray[index + 1] = value[i].Y;

            index += 2;
        }
        _gl.Uniform2fv(GetLocation(name), floatArray);
    }

    private WebGLUniformLocation GetLocation(string name)
    {
        var location = _gl.GetUniformLocation(_program, name);
        if (location == null)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }

        return location;
    }
}
