using Silk.NET.OpenGL;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace MalignEngine;

public class GLShaderResource : IShaderResource
{
    private GL _gl;
    private IShaderResourceDescriptor _descriptor;
    private GLRenderingAPI _renderAPI;

    private uint _handle;

    public GLShaderResource(GL gl, GLRenderingAPI renderAPI, IShaderResourceDescriptor descriptor)
    {
        _gl = gl;
        _descriptor = descriptor;
        _renderAPI = renderAPI;

        renderAPI.Submit(ctx =>
        {
            uint vertex = LoadShader(ShaderType.VertexShader, _descriptor.VertexShaderSource);
            uint fragment = LoadShader(ShaderType.FragmentShader, _descriptor.FragmentShaderSource);
            _handle = _gl.CreateProgram();
            _gl.AttachShader(_handle, vertex);
            _gl.AttachShader(_handle, fragment);
            _gl.LinkProgram(_handle);
            _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");
            }
            _gl.DetachShader(_handle, vertex);
            _gl.DetachShader(_handle, fragment);
            _gl.DeleteShader(vertex);
            _gl.DeleteShader(fragment);
        });
    }

    private uint LoadShader(ShaderType type, string src)
    {
        uint handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, src);
        _gl.CompileShader(handle);
        string infoLog = _gl.GetShaderInfoLog(handle);

        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }

    public void Dispose()
    {

    }

    public void Use()
    {
        _gl.UseProgram(_handle);
    }

    public bool HasUniform(string name)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        int location = _gl.GetUniformLocation(_handle, name);
        return location != -1;
    }

    public void Set(string name, int value)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        _gl.ProgramUniform1(_handle, GetLocation(name), value);
    }

    public void Set(string name, uint value)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        _gl.ProgramUniform1(_handle, GetLocation(name), value);
    }

    public void Set(string name, Matrix4x4 value)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        Matrix4x4 copy = value;

        unsafe
        {
            _gl.ProgramUniformMatrix4(_handle, GetLocation(name), 1, false, (float*)&copy);
        }
    }

    public void Set(string name, float value)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        _gl.ProgramUniform1(_handle, GetLocation(name), value);
    }

    public void Set(string name, int[] value)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        _gl.ProgramUniform1(_handle, GetLocation(name), value);
    }

    public void Set(string name, Color color)
    {
        Debug.Assert(_renderAPI.IsInRenderingThread());

        _gl.ProgramUniform4(_handle, GetLocation(name), new Vector4(color.R, color.G, color.B, color.A));
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
        _gl.ProgramUniform2(_handle, GetLocation(name), floatArray);
    }

    private int GetLocation(string name)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }

        return location;
    }
}
