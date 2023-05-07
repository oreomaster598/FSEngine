using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.IO
{
    public enum ImageFormat : byte
    {
        ARGB,
        BGRA,
        ABGR,
        RGB,
    }
    public class Image
    {


        public ImageFormat format;

        public int width;
        public int height;

        public int stride; 
        
        public byte[] data;
        public Image(Bitmap bmp)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);


            width = data.Width;
            height = data.Height;

            stride = data.Stride;

            this.data = new byte[stride * height];

            int len = stride * height;
            
            Marshal.Copy(data.Scan0, this.data, 0, len);

            bmp.UnlockBits(data);

            format = ImageFormat.ARGB;
        }
        public Image(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    format = (ImageFormat)reader.ReadByte();

                    stride = reader.ReadInt32();
                    int len = reader.ReadInt32();

                    data = reader.ReadBytes(len);

                    if(format == ImageFormat.RGB)
                    {
                        width = stride / 3;
                        height = (len / 3) / width;
                        return;
                    }

                    width = stride / 4;
                    height = (len / 4) / width;
                }
            }
        }

        public void Save(string path)
        {
            using (Stream stream = File.Open(path, FileMode.OpenOrCreate))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)format);
                    writer.Write(stride);
                    writer.Write(data.Length);
                    writer.Write(data);

                }
            }
        }
        public void SaveBmp(string path)
        {
            Bitmap bmp = new Bitmap(width,height);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            
            Marshal.Copy(this.data, 0, data.Scan0, height * stride);

            bmp.UnlockBits(data);

            bmp.Save(path);
        }
    }
}
