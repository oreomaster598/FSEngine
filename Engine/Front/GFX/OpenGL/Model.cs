using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.OpenGL
{
    
    public class Model : IDisposable
    {
        internal VertexArray vao;
        internal VertexBuffer vbo;
        int length;
        public static readonly float[] Quad =
            {
                -0.5f,  0.5f,  0, 1, // top left
                 0.5f,  0.5f,  1, 1, // top right
                -0.5f, -0.5f,  0, 0, // bottom left

                 0.5f,  0.5f,  1, 1,// top right
                 0.5f, -0.5f,  1, 0,// bottom right
                -0.5f, -0.5f,  0, 0,// bottom left
            };

        [Obsolete("Use ModelFactory.FromVertices(float[]) instead.")]
        public static Model FromVertices(float[] vertices)
        {
            Model model = new Model();
            model.length = vertices.Length / 2;
            model.vao = new VertexArray();
            model.vbo = new VertexBuffer(vertices);
            model.vao.LinkVBO(model.vbo);

            return model;
        }
        [Obsolete("Use ModelFactory.FromVertices(float[]) instead.")]
        public static Model FromVertices(float[] vertices, VAOFormat format)
        {
            Model model = new Model();
            model.length = vertices.Length / 2;
            model.vao = new VertexArray();
            model.vbo = new VertexBuffer(vertices);
            model.vao.LinkVBO(model.vbo, format);

            return model;
        }
        public void Bind()
        {
            vao.Bind();
        }
        public void Unbind()
        {
            vao.Unbind();
        }
        public void Dispose()
        {
            vao.Dispose();
            vbo.Dispose();
        }
        public void Draw()
        {
            GL.DrawArrays(PrimitiveType.Triangles, 0, length);
        }
    }
}
