using Silk.NET.OpenGL;
using System.Numerics;
using System.Text;

namespace MalignEngine
{
    public class GLShader : Shader
    {
        private uint handle;
        private GL gl;

        public GLShader(GL gl, string data)
        {
            this.gl = gl;

            StringBuilder currentShader = null;

            StringBuilder vertexShader = new StringBuilder();
            StringBuilder fragShader = new StringBuilder();

            foreach (string line in data.Split("\n"))
            {
                if (line.StartsWith("#shader vertex"))
                {
                    currentShader = vertexShader;
                }
                else if (line.StartsWith("#shader fragment"))
                {
                    currentShader = fragShader;
                }
                else if (currentShader != null)
                {
                    currentShader.AppendLine(line);
                }
            }


            uint vertex = LoadShader(ShaderType.VertexShader, vertexShader.ToString());
            uint fragment = LoadShader(ShaderType.FragmentShader, fragShader.ToString());
            handle = this.gl.CreateProgram();
            this.gl.AttachShader(handle, vertex);
            this.gl.AttachShader(handle, fragment);
            this.gl.LinkProgram(handle);
            this.gl.GetProgram(handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: {this.gl.GetProgramInfoLog(handle)}");
            }
            this.gl.DetachShader(handle, vertex);
            this.gl.DetachShader(handle, fragment);
            this.gl.DeleteShader(vertex);
            this.gl.DeleteShader(fragment);
        }

        public void Use()
        {
            gl.UseProgram(handle);
        }

        private int GetLocation(string name)
        {
            int location = gl.GetUniformLocation(handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }

            return location;
        }

        public bool HasUniform(string name)
        {
            int location = gl.GetUniformLocation(handle, name);
            return location != -1;
        }

        public void SetUniform(string name, int value)
        {
            gl.Uniform1(GetLocation(name), value);
        }

        public void SetUniform(string name, uint value)
        {
            gl.Uniform1(GetLocation(name), value);
        }

        public unsafe void SetUniform(string name, Matrix4x4 value)
        {
            gl.UniformMatrix4(GetLocation(name), 1, false, (float*)&value);
        }

        public void SetUniform(string name, float value)
        {
            gl.Uniform1(GetLocation(name), value);
        }

        public unsafe void SetUniform(string name, int[] value)
        {
            gl.Uniform1(GetLocation(name), value);
        }

        public void SetUniform(string name, Color color)
        {
            gl.Uniform4(GetLocation(name), new Vector4(color.R, color.G, color.B, color.A));
        }

        public unsafe void SetUniform(string name, Vector2[] value)
        {
            float[] floatArray = new float[value.Length * 2];
            int index = 0;
            for (int i = 0; i < value.Length; i++)
            {
                floatArray[index] = value[i].X;
                floatArray[index + 1] = value[i].Y;

                index += 2;
            }
            gl.Uniform2(GetLocation(name), floatArray);
        }

        public void Dispose()
        {
            gl.DeleteProgram(handle);
        }

        private uint LoadShader(ShaderType type, string src)
        {
            uint handle = gl.CreateShader(type);
            gl.ShaderSource(handle, src);
            gl.CompileShader(handle);
            string infoLog = gl.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
            }

            return handle;
        }
    }
}