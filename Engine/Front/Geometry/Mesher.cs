using FSEngine.Tiles;
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

        public Vector2[][] Mesh(float resolution = 0.04f)
        {
            byte[] bytes = GetMapFromBitmap(bmp);

            Result r = MarchingSquares.FindPerimeter(px, py, bmp.Width, bmp.Height, bytes);

            Vector2[] points = DouglasPeucker.Interpolate(r.vertices, (int)(r.vertices.Count * resolution)).ToArray();

            CPolygonShape shape = new CPolygonShape(points);
            shape.CutEar();

            return shape.m_aPolygons;
        }
        byte[] GetMapFromTile(Tile tile)
        {
            int width = tile.width - 10;
            int height = tile.height - 10;
            byte[] map = new byte[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                    map[y * width + x] = tile.cells[x + 5, y + 5].type > 0 ? (byte)255 : (byte)0;
                    if (px == -1 && tile.cells[x + 5, y + 5].type > 0)
                    {
                        px = x;
                        py = y;
                    }
                }
            }
            return map;
        }
        public Vector2[][] Mesh(Tile t, float resolution = 0.04f)
        {
            byte[] bytes = GetMapFromTile(t);
            Result r = MarchingSquares.FindPerimeter(px, py, t.width-10, t.height-10, bytes);

            Vector2[] points = DouglasPeucker.Interpolate(r.vertices, (int)(r.vertices.Count * resolution)).ToArray();

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
