using Box2DSharp.Common;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.GFX
{
    public class B2DDebug : IDrawer
    {

        DrawFlag _Flags = DrawFlag.DrawAABB | DrawFlag.DrawShape;
        public DrawFlag Flags { get => _Flags; set => _Flags = value; }
        public float scale_x = 720, scale_y = 480;

        public bool DrawShapeAsOutline = false;
        /// <summary>
        /// Matrix to draw in pixel space
        /// </summary>
        public void PushPixelMatrix(int x, int y)
        {
            GL.PushMatrix();

            GL.Scale( 1 / (scale_x/2), -(1/(scale_y/2)), 0);
            GL.Translate(-x - (scale_x / 2) - 5, -y - (scale_y / 2) - 5, 0);
        }
        public void DrawCircle(in Vector2 center, float radius, in Box2DSharp.Common.Color color)
        {
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
            for (float ii = 0; ii < 32; ii++)
            {
                float theta = (2.0f * 3.1415926f) * (ii / 32f);//get the current angle 

                float x = radius * (float)Math.Cos(theta);
                float y = radius * (float)Math.Sin(theta);

                GL.Vertex2(x + center.X, y + center.Y);
            }
           GL.End();
        }

        public void DrawPoint(in Vector2 p, float size, in Box2DSharp.Common.Color color)
        {
            GL.PointSize(size);
            GL.Begin(PrimitiveType.Points);
            GL.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
            GL.Vertex3(p.X, p.Y, 0.0);
            GL.End();
        }

        public void DrawPolygon(Span<Vector2> vertices, int vertexCount, in Box2DSharp.Common.Color color)
        {



            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
          
            foreach(Vector2 p in vertices)
             GL.Vertex3(p.X, p.Y, 0.0);
            GL.End();
        }

        public void DrawSegment(in Vector2 p1, in Vector2 p2, in Box2DSharp.Common.Color color)
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
            GL.Vertex2(p1.X , p1.Y);
            GL.Vertex2(p2.X, p2.Y );
            GL.End();
        }

        public void DrawSolidCircle(in Vector2 center, float radius, in Vector2 axis, in Box2DSharp.Common.Color color)
        {

            for (float ii = 0; ii < 32; ii++)
            {            
                GL.Begin(PrimitiveType.Polygon);
                GL.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * 0.5f);

                float a = (2.0f * 3.1415926f) * (ii / 32f);
                float b = (2.0f * 3.1415926f) * ((ii + 1) / 32f);

                float xa = radius * (float)Math.Cos(a);
                float ya = radius * (float)Math.Sin(a);

                float xb = radius * (float)Math.Cos(b);
                float yb = radius * (float)Math.Sin(b);

                GL.Vertex2(xa + center.X, ya + center.Y);

                GL.Vertex2(xb + center.X, yb + center.Y);

                GL.Vertex2(center.X, center.Y);

                GL.End();
            }
            
        }

        public void DrawSolidPolygon(Span<Vector2> vertices, int vertexCount, in Box2DSharp.Common.Color color)
        {
            if (DrawShapeAsOutline)
            {
                DrawPolygon(vertices, vertexCount, color);
                return;
            }


            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(color.R / 255f , color.G / 255f, color.B / 255f, color.A / 255f * 0.5f);
            foreach (Vector2 p in vertices)
            {
                GL.Vertex3(p.X, p.Y, 0.0);
            }
                
            GL.End();
        }

        public void DrawTransform(in Transform xf)
        {
            const float AxisScale = 12;

            GL.Begin(PrimitiveType.Lines);
            GL.Color4(Color.Red.R / 255f, Color.Red.G / 255f, Color.Red.B / 255f, Color.Red.A / 255f);

            Vector2 p1 = xf.Position;
            GL.Vertex2(p1.X, p1.Y);
            Vector2 p2 = p1 + AxisScale * xf.Rotation.GetXAxis();
            GL.Vertex2(p2.X, p2.Y);

            GL.Color4(Color.Green.R / 255f, Color.Green.G / 255f, Color.Green.B / 255f, Color.Green.A / 255f);
            GL.Vertex2(p1.X, p1.Y);
            p2 = p1 + AxisScale * -xf.Rotation.GetYAxis();
            GL.Vertex2(p2.X, p2.Y);
            GL.End();
        }

    }
}
