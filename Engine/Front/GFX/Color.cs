using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.GFX
{
    [StructLayout(LayoutKind.Explicit)]
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
    }
}
