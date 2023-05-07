using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace FSEngine.OpenGL
{
    public class Shader : IDisposable
    {
        public int id;
        public static string vertexShader_NOUV = "#version 330 core\nlayout (location = 0) in vec2 aPosition;\n\nuniform mat4 projection;\nuniform mat4 model;\n\nvoid main()\n{ \n       gl_Position = projection * model * vec4(aPosition.xy, 0, 1.0);\n}\0";
        public static string vertexShader = "#version 330 core\nlayout (location = 0) in vec2 aPosition;\nlayout (location = 1) in vec2 aTexCoord;\nout vec2 TexCoord;\n\nuniform mat4 projection;\nuniform mat4 model;\n\nvoid main()\n{ \n    TexCoord = aTexCoord;\n    gl_Position = projection * model * vec4(aPosition.xy, 0, 1.0);\n}\0";
        public static string fragmentShader = "#version 330 core\nout vec4 FragColor;\nin vec2 TexCoord;\n\nuniform sampler2D ourTexture;\n\nvoid main()\n { \n  FragColor = texture(ourTexture, TexCoord);\n}\0";
        public static string ParallaxShader = "#version 330 core\nout vec4 FragColor;\nin vec2 TexCoord;\nuniform sampler2D tex;\nuniform float pos;\nvoid main() { \n  	vec2 p = TexCoord + vec2(pos, 0);\n    if(p.x < 1) { \n   		FragColor = texture(tex, p); \n return;} \n    FragColor = texture(tex, vec2(p.x - 1, p.y));\n} \0";
        public static Shader Compile(string fragmentShader)
        {
            Shader shader = new Shader();

            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vertexShader);
            GL.CompileShader(vs);

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fragmentShader);
            GL.CompileShader(fs);

            shader.id = GL.CreateProgram();
            GL.AttachShader(shader.id, vs);
            GL.AttachShader(shader.id, fs);

            GL.LinkProgram(shader.id);

            GL.DetachShader(shader.id, vs);
            GL.DetachShader(shader.id, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            return shader;
        }
        public static Shader Compile(string fragmentShader, string vertexShader)
        {
            Shader shader = new Shader();

            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vertexShader);
            GL.CompileShader(vs);

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fragmentShader);
            GL.CompileShader(fs);

            shader.id = GL.CreateProgram();
            GL.AttachShader(shader.id, vs);
            GL.AttachShader(shader.id, fs);

            GL.LinkProgram(shader.id);

            GL.DetachShader(shader.id, vs);
            GL.DetachShader(shader.id, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            return shader;
        }
        public void Dispose()
        {
            GL.DeleteProgram(id);
        }
        public void Bind()
        {
            GL.UseProgram(id);
        }
        public void Unbind()
        {
            GL.UseProgram(0);
        }

        public void SetMatrix4x4(string name, Matrix4x4 mat)
        {
            int loc = GL.GetUniformLocation(id, name);
            GL.UniformMatrix4(loc, 1, false, GetMatrix4x4Values(mat));
        }
        public void SetFloat(string name, float f)
        {
            int loc = GL.GetUniformLocation(id, name);
            GL.Uniform1(loc, f);
        }
        public void SetInt(string name, int value)
        {
            int loc = GL.GetUniformLocation(id, name);

            GL.Uniform1(loc, value);
        }
        private float[] GetMatrix4x4Values(Matrix4x4 m)
        {
            return new float[]
            {
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
            };
        }
    }
}
