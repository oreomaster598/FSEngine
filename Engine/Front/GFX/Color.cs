using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.GFX
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Color
    {
        [FieldOffset(0)]
        public byte A;
        [FieldOffset(1)]
        public byte R;
        [FieldOffset(2)]
        public byte G;
        [FieldOffset(3)]
        public byte B;

        [FieldOffset(0)]
        public uint ARGB;

        public static Color Red = FromArgb(191, 19, 10);
        public static Color DarkRed = FromArgb(105, 0, 0);
        public static Color Green = FromArgb(0, 184, 6);
        public static Color Orange = FromArgb(242, 104, 5);
        public static Color Purple = FromArgb(129, 50, 168);
        public Color(int a, int r, int g, int b)
        {
            ARGB = 0;
            A = (byte)a;
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
        }
        public static Color FromSDColor(System.Drawing.Color color)
        {
            Color c = new Color();
            c.A = (byte)color.A;
            c.R = (byte)color.R;
            c.G = (byte)color.G;
            c.B = (byte)color.B;
            return c;
        }
        public static Color FromArgb(int a, int r, int g, int b)
        {
            Color c = new Color();
            c.A = (byte)a;
            c.R = (byte)r;
            c.G = (byte)g;
            c.B = (byte)b;
            return c;
        }
        public static Color FromArgb(float a, float r, float g, float b)
        {
            Color c = new Color();
            c.A = (byte)(255 * a);
            c.R = (byte)(255 * r);
            c.G = (byte)(255 * g);
            c.B = (byte)(255 * b);
            return c;
        }
        public static Color FromArgb(int r, int g, int b)
        {
            Color c = new Color();
            c.A = (byte)255;
            c.R = (byte)r;
            c.G = (byte)g;
            c.B = (byte)b;
            return c;
        }
        public static float HueToRGB(float v1, float v2, float vH)
        {
            if (vH < 0)
                vH += 1;

            if (vH > 1)
                vH -= 1;

            if ((6 * vH) < 1)
                return (v1 + (v2 - v1) * 6 * vH);

            if ((2 * vH) < 1)
                return v2;

            if ((3 * vH) < 2)
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

            return v1;
        }
        public static Color FromHSL(int h, float s, float l)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (s == 0)
            {
                r = g = b = (byte)(l * 255);
            }
            else
            {
                float v1, v2;
                float hue = (float)h / 360;

                v2 = (l < 0.5) ? (l * (1 + s)) : ((l + s) - (l * s));
                v1 = 2 * l - v2;

                r = (byte)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
                g = (byte)(255 * HueToRGB(v1, v2, hue));
                b = (byte)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
            }

            return Color.FromArgb(r, g, b);
        }


        public static Color operator +(Color a, Color b)
        {
            return new Color(a.A + b.A, a.R + b.R, a.G + b.G, a.B + b.B);
        }
        public static Color operator -(Color a, Color b)
        {
            return new Color(a.A - b.A, a.R - b.R, a.G - b.G, a.B - b.B);
        }
        public static Color operator *(Color a, Color b)
        {
            return new Color(a.A * b.A, a.R* b.R, a.G * b.G, a.B * b.B);
        }
        public static Color operator /(Color a, Color b)
        {
            return new Color(a.A / b.A, a.R / b.R, a.G / b.G, a.B / b.B);
        }
        public static Color operator *(Color a, int scalar)
        {
            return new Color(a.A * scalar, a.R * scalar, a.G * scalar, a.B * scalar);
        }
        public static Color operator /(Color a, int scalar)
        {
            return new Color(a.A / scalar, a.R / scalar, a.G / scalar, a.B / scalar);
        }
        public static Color operator +(Color a, int scalar)
        {
            return new Color(a.A + scalar, a.R + scalar, a.G + scalar, a.B + scalar);
        }
        public static Color operator -(Color a, int scalar)
        {
            return new Color(a.A - scalar, a.R - scalar, a.G - scalar, a.B - scalar);
        }
    }
    public struct ColorF
    {

        public float A;
         
        public float R;

        public float G;

        public float B;


        public ColorF(float a, float r, float g, float b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }
        public static ColorF FromColor(Color color)
        {
            ColorF c = new ColorF();
            c.A = color.A / 255f;
            c.R = color.R / 255f;
            c.G = color.G / 255f;
            c.B = color.B / 255f;
            return c;
        }
        public Color ToColor()
        {
            return Color.FromArgb(A,R,G,B);
        }
        public static ColorF FromArgb(float a, float r, float g, float b)
        {
            ColorF c = new ColorF();
            c.A = a;
            c.R = r;
            c.G = g;
            c.B = b;
            return c;
        }

        public static ColorF FromArgb(float r, float g, float b)
        {
            ColorF c = new ColorF();
            c.A = 1;
            c.R = r;
            c.G = g;
            c.B = b;
            return c;
        }
        public static float HueToRGB(float v1, float v2, float vH)
        {
            if (vH < 0)
                vH += 1;

            if (vH > 1)
                vH -= 1;

            if ((6 * vH) < 1)
                return (v1 + (v2 - v1) * 6 * vH);

            if ((2 * vH) < 1)
                return v2;

            if ((3 * vH) < 2)
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

            return v1;
        }
        public static ColorF FromHSL(int h, float s, float l)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (s == 0)
            {
                r = g = b = (byte)(l * 255);
            }
            else
            {
                float v1, v2;
                float hue = (float)h / 360;

                v2 = (l < 0.5) ? (l * (1 + s)) : ((l + s) - (l * s));
                v1 = 2 * l - v2;

                r = (byte)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
                g = (byte)(255 * HueToRGB(v1, v2, hue));
                b = (byte)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
            }

            return ColorF.FromArgb(r/255f, g/255f, b/255f);
        }


        public static ColorF operator +(ColorF a, ColorF b)
        {
            return new ColorF(a.A + b.A, a.R + b.R, a.G + b.G, a.B + b.B);
        }
        public static ColorF operator -(ColorF a, ColorF b)
        {
            return new ColorF(a.A - b.A, a.R - b.R, a.G - b.G, a.B - b.B);
        }
        public static ColorF operator *(ColorF a, ColorF b)
        {
            return new ColorF(a.A * b.A, a.R * b.R, a.G * b.G, a.B * b.B);
        }
        public static ColorF operator /(ColorF a, ColorF b)
        {
            return new ColorF(a.A / b.A, a.R / b.R, a.G / b.G, a.B / b.B);
        }
        public static ColorF operator *(ColorF a, float scalar)
        {
            return new ColorF(a.A * scalar, a.R * scalar, a.G * scalar, a.B * scalar);
        }
        public static ColorF operator /(ColorF a, float scalar)
        {
            return new ColorF(a.A / scalar, a.R / scalar, a.G / scalar, a.B / scalar);
        }
        public static ColorF operator +(ColorF a, float scalar)
        {
            return new ColorF(a.A + scalar, a.R + scalar, a.G + scalar, a.B + scalar);
        }
        public static ColorF operator -(ColorF a, float scalar)
        {
            return new ColorF(a.A - scalar, a.R - scalar, a.G - scalar, a.B - scalar);
        }
    }
}
