using FSEngine.OpenGL;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.Concurrency
{
    public enum PixelFormat
    {
        ARGB,
        RGBA,
        ABGR,
        BGRA,
        RGB,
    }
    public unsafe class TSBitmap
    {
        internal Byte[] buffer;
        public UInt32 Width;
        public UInt32 Height;
        UInt32 stride;
        public Texture glTexture;
        public int RowBytes => (int)(Width * stride);
        public TSBitmap(UInt32 Width, UInt32 Height)
        {
            this.Height = Height;
            this.Width = Width;

            stride = 4;
            buffer = new byte[Width * Height * stride];
        }
        public TSBitmap(string path)
        {

            using (Bitmap bmp = new Bitmap(path))
            {
                Width = (UInt32)bmp.Width;
                Height = (UInt32)bmp.Height;
                stride = 4;
                buffer = new byte[Width * Height * stride];

                BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);


                Marshal.Copy(data.Scan0, buffer, 0, RowBytes * bmp.Height);
                bmp.UnlockBits(data);
            }


        }
        public unsafe void CopyTo(TSBitmap bmp)
        {
            fixed(byte* buf1 = buffer)
                fixed(byte* buf2 = bmp.buffer)
                    MemoryMgr.Memory.Copy((IntPtr)buf1, (IntPtr)buf2, buffer.Length);
        }
        public void Clear()
        {
            Array.Clear(buffer, 0, buffer.Length);
        }
        public Color GetPixel(UInt32 x, UInt32 y, PixelFormat format = PixelFormat.ARGB)
        {
            x *= stride;
            y *= stride;
            if (format == PixelFormat.ABGR)
            {
                byte r = buffer[y * Width + x];
                byte g = buffer[y * Width + x + 1];
                byte b = buffer[y * Width + x + 2];
                byte a = buffer[y * Width + x + 3];
                return Color.FromArgb(a, b, g, r);
            }
            else if (format == PixelFormat.ARGB)
            {
                byte r = buffer[y * Width + x];
                byte g = buffer[y * Width + x + 1];
                byte b = buffer[y * Width + x + 2];
                byte a = buffer[y * Width + x + 3];
                return Color.FromArgb(a, r, g, b);
            }
            return Color.Transparent;

        }
        public bool IsTranlucentPixel(UInt32 x, UInt32 y, PixelFormat format = PixelFormat.ARGB)
        {
            x *= stride;
            y *= stride;
            if (y * Width + x + 3 > buffer.Length)
                return false;
            return buffer[y * Width + x + 3] < 255;

        }
        public Color GetPixel(int x, int y) => GetPixel((uint)x, (uint)y);
        public void SetPixel(int x, int y, GFX.Color c) => SetPixel((uint)x, (uint)y, c);
        public void SetPixel(UInt32 x, UInt32 y, GFX.Color c)
        {
            x *= stride;
            y *= stride;
            buffer[y * Width + x] = c.B;
            buffer[y * Width + x + 1] = c.G;
            buffer[y * Width + x + 2] = c.R;
            buffer[y * Width + x + 3] = c.A;
        }
        // adds the color
        public void MulPixel(UInt32 x, UInt32 y, float r,float g,float b)
        {
            x *= stride;
            y *= stride;
            buffer[y * Width + x] = (byte)(buffer[y * Width + x] * b);
            buffer[y * Width + x + 1] = (byte)(buffer[y * Width + x + 1] * g);
            buffer[y * Width + x + 2] = (byte)(buffer[y * Width + x + 2] * r);
        }
        public void MulPixel(UInt32 x, UInt32 y, GFX.Color c)
        {
            x *= stride;
            y *= stride;
            buffer[y * Width + x] = (byte)((float)buffer[y * Width + x] * (255f / c.B));
            buffer[y * Width + x + 1] = (byte)((float)buffer[y * Width + x + 1] * (255f / c.G));
            buffer[y * Width + x + 2] = (byte)((float)buffer[y * Width + x + 2] * (255f / c.R));
        }
        public void SetPixelSafe(Int32 x, Int32 y, GFX.Color c) => SetPixelSafe((uint)x, (uint)y, c);
        public void SetPixelSafe(UInt32 x, UInt32 y, GFX.Color c)
        {
            x *= stride;
            y *= stride;
            if (y * Width + x + 3 > buffer.Length)
                return;
            buffer[y * Width + x] = c.B;
            buffer[y * Width + x + 1] = c.G;
            buffer[y * Width + x + 2] = c.R;
            buffer[y * Width + x + 3] = c.A;
        }
        public Texture GetTexture()
        {
            if(glTexture == null)
            {
                fixed(byte* ptr = buffer)
                    glTexture = Texture.FromPixels((IntPtr)ptr, (int)Height, (int)Width);
            }
            return glTexture;
        }
        public void UpdateTexture()
        {
            GetTexture(); // Make sure there is a texture to update
            fixed (byte* ptr = buffer)
            {
                GL.BindTexture(TextureTarget.Texture2D, glTexture.id);
                //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)Width, (int)Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)ptr);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (int)Width, (int)Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)ptr);
            }
                
        }
        public Bitmap ToBitmap()
        {
            Bitmap bmp = new Bitmap((Int32)Width, (Int32)Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Marshal.Copy(buffer, 0, data.Scan0, RowBytes * bmp.Height);
            bmp.UnlockBits(data);
            return bmp;
        }
    }
}
