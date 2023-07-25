using FSEngine.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoryMgr;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
                pb.SetPixelSafe(x, _y, clr);
            }
            for (x = _x; x < _x + width; x++)
            {
                pb.SetPixelSafe(x, _y + height, clr);
            }

            for (y = _y; y < _y + height; y++)
            {
                pb.SetPixelSafe(_x, y, clr);
            }
            for (y = _y; y < _y + height; y++)
            {
                pb.SetPixelSafe(_x + width, y, clr);
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
                        pb.SetPixelSafe(_x + x, _y + y, border);
                        continue;
                    }
                    pb.SetPixelSafe(_x + x, _y + y, inside);
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
                        pb.SetPixelSafe(_x + x, _y + y, inside);
                    else if((int)hyp == (int)radius)
                        pb.SetPixelSafe(_x + x, _y + y, border);

                }
            }
        }


        GCHandle pb_handle;

        public unsafe void BeginThreadSafe()
        {
           
           pb_handle = GCHandle.Alloc(pb.buffer, GCHandleType.Pinned);
        }
        public unsafe void EndThreadSafe()
        {
            pb_handle.Free();
            pb_handle = default(GCHandle);
        }

        /// <summary>
        /// Time complexity is 0.00000075ms per pixel (or 1333333 pixels per ms).
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public unsafe void Blit(TSBitmap bitmap, int x, int y)
        {
            int space_x = (int)pb.Width - (x + (int)bitmap.Width);
            int space_y = (int)pb.Height - (y + (int)bitmap.Height);

            int width = (int)bitmap.Width;
            int height = (int)bitmap.Height;

            if (space_x < 0)
                width += space_x;

            if(space_y < 0)
                height += space_y;

            fixed (byte* src = bitmap.buffer)
                fixed (byte* dst = &pb.buffer[0])
                    for (int i = 0; (i < bitmap.Height && i + y < pb.Height); i++)
                    {
                        Memory.CopyMemory((IntPtr)(&(dst[((i + y) * pb.Width * 4) + x * 4])), (IntPtr)(&(src[i * bitmap.Width * 4])), (uint)width * 4);
                    }
        }
        
        /// <summary>
         /// Time complexity is 0.00000075ms per pixel (or 1333333 pixels per ms).
         /// </summary>
         /// <param name="bitmap"></param>
         /// <param name="x"></param>
         /// <param name="y"></param>
        public unsafe void BlitSafe(TSBitmap bitmap, int x, int y)
        {
            int space_x = (int)pb.Width - (x + (int)bitmap.Width);
            int space_y = (int)pb.Height - (y + (int)bitmap.Height);

            int width = (int)bitmap.Width;
            int height = (int)bitmap.Height;

            if (space_x < 0)
                width += space_x;

            if (space_y < 0)
                height += space_y;

            byte* dst = (byte*)pb_handle.AddrOfPinnedObject().ToPointer();

            fixed (byte* src = bitmap.buffer)
                for (int i = 0; (i < bitmap.Height && i + y < pb.Height); i++)
                {
                    Memory.CopyMemory((IntPtr)(&(dst[((i + y) * pb.Width * 4) + x * 4])), (IntPtr)(&(src[i * bitmap.Width * 4])), (uint)width * 4);
                }
        }
    }
}
