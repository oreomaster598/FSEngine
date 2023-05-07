using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using OpenTK.Graphics.OpenGL;

namespace FSEngine.OpenGL
{
    public class VertexArray
    {
        public int id;

        public VertexArray()
        {
            id = GL.GenVertexArray();
        }
        public void Dispose()
        {
            GL.DeleteVertexArray(id);
        }
        public void Bind()
        {
            GL.BindVertexArray(id);
        }
        public void Unbind()
        {
            GL.BindVertexArray(0);
        }
        public unsafe void LinkVBO(VertexBuffer vbo, VAOFormat format = VAOFormat.XYUV)
        {
            Bind();
            vbo.Bind();
            if(format == VAOFormat.XYUV)
            {
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, (IntPtr)0);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, (IntPtr)(sizeof(float) * 2));
                GL.EnableVertexAttribArray(1);
            }
            else if (format == VAOFormat.XY)
            {
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, (IntPtr)0);
                GL.EnableVertexAttribArray(0);
            }

            vbo.Unbind();
            Unbind();
        }
    }
    public enum VAOFormat
    {
        XYUV,
        XY,
    }
    public class VertexBuffer
    {
        public int id;
        public unsafe VertexBuffer(float[] buffer)
        {
            id = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, id);

            fixed (float* v = &buffer[0])
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * buffer.Length, (IntPtr)v, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, id);
        }

        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        public void Dispose()
        {
            GL.DeleteBuffer(id);
        }
    }

    public class FrameBuffer
    {
        int id = -1;
        int texture = -1;
        public int Width;
        public int Height;

        public Texture tex;

        public FrameBuffer(int width, int height)
        {
            Width = width;
            Height = height;

            texture = GL.GenTexture();           
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, 0x8370);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, 0x8370);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 0x2600);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 0x2600);

            tex = Texture.FromHandle(texture, width, height);

            id = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        public void Resize(int width, int  height)
        {
            Width = width;
            Height = height;

            tex.width = width;
            tex.height = height;

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, 0x8370);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, 0x8370);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 0x2600);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 0x2600);
        }
        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
        }
        public void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        public void Dispose()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteFramebuffer(id);
            GL.DeleteTexture(texture);


        }        
      
        public void BeginDrawing()
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, id);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.PushAttrib(AttribMask.ViewportBit);

            GL.Viewport(0, 0, Width, Height);
        }

        public void EndDrawing()
        {
            GL.PopAttrib();
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0); // disable rendering into the FBO
        }
    }
}
