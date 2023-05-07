using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.OpenGL
{
    public enum InterpolationMode
    {
        Point = 0x2600,
        Bilinear = 0x2601,

    }
    public struct TextureInfo
    {
        public InterpolationMode interpolation;
        public bool mipmaps;
    }
    public class Texture : IDisposable
    {
        public int id;
        

        public int width;
        public int height;
        public static unsafe Texture FromFile(string path)
        {
            Texture tex = new Texture();

            Bitmap bmp = new Bitmap(path);
            tex.width = bmp.Width;
            tex.height = bmp.Height;
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            tex.id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex.id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, 0x8370);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, 0x8370);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 0x2600);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 0x2601);

            bmp.UnlockBits(data);
            bmp.Dispose();
            return tex;
        }
        public static unsafe Texture FromPixels(IntPtr pixels, int height, int width)
        {
            Texture tex = new Texture();

            
            tex.id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex.id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, 0x8370);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, 0x8370);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 0x2600);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 0x2601);

            return tex;
        }
        public static unsafe Texture FromHandle(int handle, int width, int height)
        {
            Texture tex = new Texture();
            tex.id = (int)handle;
            return tex;
        }
        public static explicit operator IntPtr(Texture tex) => (IntPtr)tex.id;
        public void Bind(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, id);
        }
        public void Unbind(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        public void Dispose()
        {
            GL.DeleteTexture(id);
        }
    }
}
