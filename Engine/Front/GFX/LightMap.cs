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

namespace FSEngine.GFX
{
    public struct Light
    {
        public uint x;
        public uint y;
        public float brightness;
        public float r;
        public float g;
        public float b;
    }
    public unsafe class LightMap
    {
        internal Byte[] buffer;
        public UInt32 Width;
        public UInt32 Height;
        UInt32 stride;
        public Texture glTexture;
        public int RowBytes => (int)(Width * stride);
        public LightMap(UInt32 Width, UInt32 Height)
        {
            this.Height = Height;
            this.Width = Width;

            stride = 4;
            buffer = new byte[Width * Height * stride];
        }
        public void Clear()
        {
            Array.Clear(buffer, 0, buffer.Length);
        }
        public void PlaceLight(Light light)
        {
            uint x = light.x, y = light.y;
            x *= stride;
            y *= stride;
            if (y * Width + x + 3 > buffer.Length)
                return;
            buffer[y * Width + x] = (byte)(255f * light.b);
            buffer[y * Width + x + 1] = (byte)(255f * light.g);
            buffer[y * Width + x + 2] = (byte)(255f * light.r);
            buffer[y * Width + x + 3] = (byte)(255f * light.brightness);
        }
        public Texture GetTexture()
        {
            if (glTexture == null)
            {
                fixed (byte* ptr = buffer)
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
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)Width, (int)Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)ptr);
            }

        }
    }
}
