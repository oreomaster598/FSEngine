using FSEngine.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.GFX
{
    public class CPUGraphics
    {
        internal TSBitmap pb;

        public CPUGraphics(TSBitmap surface)
        {
            this.pb = surface;
        }


        public void DrawRect(int x, int y, int width, int height, Color clr)
        {
            int _x = x, _y = y;

            for (x = _x; x < _x + width; x++)
            {
                pb.SetPixel(x, _y, clr);
            }
            for (x = _x; x < _x + width; x++)
            {
                pb.SetPixel(x, _y + height, clr);
            }

            for (y = _y; y < _y + height; y++)
            {
                pb.SetPixel(_x, y, clr);
            }
            for (y = _y; y < _y + height; y++)
            {
                pb.SetPixel(_x + width, y, clr);
            }
        }
        public void DrawRectFill(int _x, int _y, int width, int height, Color border, Color inside)
        {
            //(X >= 0 && X <= maxX && Y >= 0 && Y <= maxY) && ((X % maxX) == 0 || (Y % maxY) == 0)

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if((x >= 0 && x <= width - 1 && y >= 0 && y <= height - 1) && ((x % (width - 1)) == 0 || (y % (height - 1)) == 0))
                    {
                        pb.SetPixel(_x + x, _y + y, border);
                        continue;
                    }
                    pb.SetPixel(_x + x, _y + y, inside);
                }
            }
        }
        public void DrawCircle(int _x, int _y, double radius, Color border)
        {
            //(X >= 0 && X <= maxX && Y >= 0 && Y <= maxY) && ((X % maxX) == 0 || (Y % maxY) == 0)

            for (int x = 0; x < radius * 2 + 1; x++)
            {
                for (int y = 0; y < radius * 2 + 1; y++)
                {
                    double hyp = Math.Sqrt(Math.Pow(x - radius, 2) + Math.Pow(y - radius, 2));
                     if ((int)hyp == (int)radius)
                        pb.SetPixelSafe((uint)_x + (uint)x, (uint)_y + (uint)y, border);

                }
            }
        }
        public void DrawCircleFill(int _x, int _y, double radius, Color border, Color inside)
        {
            //(X >= 0 && X <= maxX && Y >= 0 && Y <= maxY) && ((X % maxX) == 0 || (Y % maxY) == 0)

            for (int x = 0; x < radius * 2 + 1; x++)
            {
                for (int y = 0; y < radius * 2 + 1; y++)
                {
                    double hyp = Math.Sqrt(Math.Pow(x - radius, 2) + Math.Pow(y - radius , 2));
                    if (hyp < radius)
                        pb.SetPixel(_x + x, _y + y, inside);
                    else if((int)hyp == (int)radius)
                        pb.SetPixel(_x + x, _y + y, border);

                }
            }
        }

    }
}
