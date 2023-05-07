using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.Geometry
{
    public class Mesher
    {

        int px = -1, py;

        Bitmap bmp;
        public Mesher(Bitmap bmp)
        {
            this.bmp = bmp;
        }

        public Vector2[][] Mesh(int resolution = 25)
        {
            byte[] bytes = GetMapFromBitmap(bmp);

            Result r = MarchingSquares.FindPerimeter(px, py, bmp.Width, bmp.Height, bytes);

            Vector2[] points = DouglasPeucker.Interpolate(r.vertices, r.vertices.Count / resolution).ToArray();

            CPolygonShape shape = new CPolygonShape(points);
            shape.CutEar();

            return shape.m_aPolygons;
        }

        byte[] GetMapFromBitmap(Bitmap bmp)
        {
            byte[] map = new byte[bmp.Width * bmp.Height];
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {

                    map[y * bmp.Width + x] = bmp.GetPixel(x, y).A > 128 ? (byte)255 : (byte)0;
                    if (px == -1 && bmp.GetPixel(x, y).A > 128)
                    {
                        px = x;
                        py = y;
                    }
                }
            }
            return map;
        }

    }
}
