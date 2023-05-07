using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows;

namespace FSEngine
{
    /// <summary>
    /// Can be used as a float vector or a int vector.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector2FI
    {
        [FieldOffset(4)] public int _X;
        [FieldOffset(0)] public int _Y;
        [FieldOffset(4)] public float X;
        [FieldOffset(0)] public float Y;

        public Vector2FI(float x, float y)
        {
            _X = 0;
            _Y = 0;
            this.X = x;
            this.Y = y;
        }
        public Vector2FI(int x, int y)
        {
            this.X = 0;
            this.Y = 0;
            _X = x;
            _Y = x;
        }
        public override int GetHashCode()
        {
            return (_X * 0x1f1f1f1f) ^ _Y;
        }

        public static Vector2FI operator +(Vector2FI a, Vector2FI b)
        {
            return new Vector2FI(a.X + b.X, a.Y + b.Y);
        }
        public static Vector2FI operator -(Vector2FI a, Vector2FI b)
        {
            return new Vector2FI(a.X - b.X, a.Y - b.Y);
        }
        public static Vector2FI operator *(Vector2FI a, Vector2FI b)
        {
            return new Vector2FI(a.X * b.X, a.Y * b.Y);
        }
        public static Vector2FI operator /(Vector2FI a, Vector2FI b)
        {
            return new Vector2FI(a.X / b.X, a.Y / b.Y);
        }
    }

    public class Delay
    {
        long start;
        long ms;
        public Delay(int ms)
        {
            this.ms = ms;
        }


        public bool Tick()
        {
            if(Time.sw.ElapsedMilliseconds - start >= ms)
            {
                start = Time.sw.ElapsedMilliseconds;
                return true;
            }
            return false;
        }
    }
    public class HistoryStack<T>
    {
        private List<T> items = new List<T>();
        //public List<T> Items => items.ToList();
        public int Capacity { get; }
        public int Count => items.Count;
        public HistoryStack(int capacity)
        {
            Capacity = capacity;
        }

        public T this[int index]
        {
            get
            {
                return items[index];
            }
            set
            {
                items[index] = value;
            }
        }
        public T[] ToArray()
        {
            return items.ToArray();
        }
        public void Push(T item)
        {
            if (items.Count == Capacity)
            {
                items.RemoveAt(0);
                items.Add(item);
                return;
            }
            items.Add(item);
        }

        public T Pop()
        {
            if (items.Count == 0)
            {
                return default;
            }
            
            T item = items[items.Count-1];
            items.RemoveAt(items.Count-1);
            return item;
        }
    }

    public class TSRandom
    {
        //a = 16807;
        //m = 2147483647;
        //seed = (a* seed) mod m;
        //random = seed / m;
        volatile int seed = (int)DateTime.Now.Ticks;
        public int Next()
        {
            unchecked
            {
                seed = (16807 * seed) % 2147483647;
                return seed;
            }
        }
        public double NextDouble()
        {
            unchecked
            {
                seed = (16807 * seed) % 2147483647;
                return ((uint)seed / 2) / 2147483647d;
            }
        }
    }
    public static class Utils
    {

        public static uint ToRgba(this System.Drawing.Color c)
        {
            uint value = c.R;
            value |= (uint)c.G << 8;
            value |= (uint)c.B << 16;
            value |= (uint)c.A << 24;

            return value;
        }
        public static Vector2 RotateUV(Vector2 uv, float rotation, Vector2 mid)
        {
            return new Vector2(
              (float)Math.Round((float)Math.Cos(rotation) * (uv.X - mid.X) + (float)Math.Sin(rotation) * (uv.Y - mid.Y) + mid.X),
              (float)Math.Round((float)Math.Cos(rotation) * (uv.Y - mid.Y) - (float)Math.Sin(rotation) * (uv.X - mid.X) + mid.Y)
            );
        }
        public static double NextDouble(this Random r, double min, double max)
        {
            return min + (r.NextDouble() * (max - min));
        }
        public static float Interpolate(float target, float value, float resolution = 4)
        {
            return value + ((target - value) / resolution);
        }
        public static float NextFloat(this Random r, float min, float max)
        {
            return min + ((float)r.NextDouble() * (max - min));
        }
        public static int Clamp(int n, int min, int max)
        {
            if (n < min) return min;
            if (n > max) return max;
            return n;
        }

        public static T PickRandom<T>(T a, T b)
        {
            if (Game.rng.Next(0, 2) == 0)
                return a;
            return b;
        }
        public static T PickRandom<T>(T a, T b, T c)
        {
            int i = Game.rng.Next(0, 3);
            if (i == 0)
                return a;
            if (i == 1)
                return b;
            return c;
        }

        public static void CloseConsole()
        {
            Kernel.FreeConsole();
        }
        public static void OpenConsole()
        {
            bool consoleAttached = true;
            if ((Kernel.AttachConsole(Kernel.ATTACH_PARRENT) == 0 && Marshal.GetLastWin32Error() != Kernel.ERROR_ACCESS_DENIED))
            {
                consoleAttached = Kernel.AllocConsole() != 0;
            }

            if (consoleAttached)
            {
                InitializeOutStream();
                InitializeInStream();
            }
        }

        private static void InitializeOutStream()
        {
            FileStream fs = CreateFileStream("CONOUT$", Kernel.GENERIC_WRITE, Kernel.FILE_SHARE_WRITE, FileAccess.Write);
            if (fs != null)
            {
                StreamWriter writer = new StreamWriter(fs) { AutoFlush = true };
                Console.SetOut(writer);
                Console.SetError(writer);
            }
        }

        private static void InitializeInStream()
        {
            FileStream fs = CreateFileStream("CONIN$", Kernel.GENERIC_READ, Kernel.FILE_SHARE_READ, FileAccess.Read);
            if (fs != null)
            {
                Console.SetIn(new StreamReader(fs));
            }
        }

        private static FileStream CreateFileStream(string name, uint win32DesiredAccess, uint win32ShareMode,
                                FileAccess dotNetFileAccess)
        {
            SafeFileHandle file = new SafeFileHandle(Kernel.CreateFileW(name, win32DesiredAccess, win32ShareMode, IntPtr.Zero, Kernel.OPEN_EXISTING, Kernel.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero), true);
            if (!file.IsInvalid)
            {
                FileStream fs = new FileStream(file, dotNetFileAccess);
                return fs;
            }
            return null;
        }
    }
}
