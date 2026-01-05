using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace TessellationDemo
{
    public class ShaderProgram : IDisposable
    {
        public int Handle { get; private set; }

        public ShaderProgram(string vertexPath, string tessControlPath, string tessEvalPath, string fragmentPath)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexPath);
            int tessControlShader = CompileShader(ShaderType.TessControlShader, tessControlPath);
            int tessEvalShader = CompileShader(ShaderType.TessEvaluationShader, tessEvalPath);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentPath);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, tessControlShader);
            GL.AttachShader(Handle, tessEvalShader);
            GL.AttachShader(Handle, fragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                throw new Exception($"Program failed to link: {infoLog}");
            }

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, tessControlShader);
            GL.DetachShader(Handle, tessEvalShader);
            GL.DetachShader(Handle, fragmentShader);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(tessControlShader);
            GL.DeleteShader(tessEvalShader);
            GL.DeleteShader(fragmentShader);
        }

        private int CompileShader(ShaderType type, string path)
        {
            string source = File.ReadAllText(path);
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Shader {path} failed to compile: {infoLog}");
            }

            return shader;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetUniformLocation(string name)
        {
            return GL.GetUniformLocation(Handle, name);
        }

        public void SetFloat(string name, float value)
        {
            int location = GetUniformLocation(name);
            GL.Uniform1(location, value);
        }

        public void SetBool(string name, bool value)
        {
            int location = GetUniformLocation(name);
            GL.Uniform1(location, value ? 1 : 0);
        }

        public void SetMatrix4(string name, OpenTK.Mathematics.Matrix4 matrix)
        {
            int location = GetUniformLocation(name);
            GL.UniformMatrix4(location, false, ref matrix);
        }

        public void SetVector3(string name, OpenTK.Mathematics.Vector3 vector)
        {
            int location = GetUniformLocation(name);
            GL.Uniform3(location, vector);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}
