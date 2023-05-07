using FSEngine.CellSystem.Effects.Particles;
using FSEngine.Concurrency;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using PixelFormat = FSEngine.Concurrency.PixelFormat;

namespace FSEngine.CellSystem
{
    public enum FillMode
    {
        Fill,
        Replace,
        Hallow,
        Erase,


    }
    public class TextureSampler
    {
        private UInt32 w;
        private UInt32 h;
        public TSBitmap sampler;

        public TextureSampler(string path)
        {
            sampler = new TSBitmap(path);
            w = sampler.Width;
            h = sampler.Height;
        }
        public Color Sample(Int32 x, Int32 y, Int32 camx, Int32 camy) => sampler.GetPixel((UInt32)((x + camx) % w), (UInt32)((y + camy) % h), PixelFormat.ABGR);
        public Color Sample(Int32 x, Int32 y) => sampler.GetPixel((UInt32)(x % w), (UInt32)(y % h), PixelFormat.ABGR);
    }
    public class Sampler
    {
        internal static Dictionary<Int16, Sampler> mappings = new Dictionary<Int16, Sampler>();

        public TSBitmap sampler;
        public IntPtr texture = IntPtr.Zero;
        public Int16 type;
        private UInt32 w;
        private UInt32 h;
        private Int16 life;
        public Int64 Count = 10000;
        public bool Consume = true;
        public static bool replace = false;
        public static FillMode mode;
        public static uint hardcap = 1000000;

     

        public static Sampler GetSampler(Int16 type)
        {
            if(mappings.ContainsKey(type))
                return mappings[type];
            return new Sampler() { Count = 0};
        }
        public static void GenTextures()
        {
            foreach(Sampler s in mappings.Values)
            {
                s.texture = (IntPtr)s.sampler.GetTexture();
            }
        }
        public static void DeleteSamplers()
        {
            foreach (Sampler s in mappings.Values)
            {
                GL.DeleteTexture((int)s.texture);
            }
            mappings.Clear();
        }
        public Sampler(string path)
        {
            sampler = new TSBitmap(path);
            w = (UInt32)sampler.Width;
            h = (UInt32)sampler.Height;
        }
        public Sampler()
        {
        }
        public Sampler(string path, Int16 type)
        {
            sampler = new TSBitmap(path);
            w = (UInt32)sampler.Width;
            h = (UInt32)sampler.Height;
            this.type = type;
            life = 0;
            if (life == 0) life = -1;
            Consume = false;
        }

        public static unsafe void Map(Sampler sampler, Int16 type)
        {
            sampler.type = type;
            sampler.life =0;
            if (sampler.life == 0) sampler.life = -1;
            mappings.Add(type, sampler);
        }

        public Color Sample(Int32 x, Int32 y) => sampler.GetPixel((UInt32)(x % w), (UInt32)(y % h), PixelFormat.ABGR);
        public Color SampleUV(Int32 x, Int32 y) => sampler.GetPixel((UInt32)(x), (UInt32)(y), PixelFormat.ABGR);

        /// <summary>
        /// Creates a cell with a color from the sampler
        /// </summary>
        /// <param name="world"></param>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void CreateCell(CellWorld world, Int32 x, Int32 y)
        {
            Material mat = Materials.Get(type);
            if (Count == 0)
                return;
            Cell c = new Cell();


            c.life = life;
            c.type = type;
            Color cl = Sample(x, y);
            c.A = cl.A;
            c.R = cl.R;
            c.G = cl.G;
            c.B = cl.B;

            world.SetCell((Int32)x, (Int32)y, c);
            world.Wake(x, y);
            if(Consume)
            Count--;
        }
        public static Cell GetCell(short type, GFX.Color cl)
        {
            Material mat = Materials.Get(type);
            Cell c = new Cell();


            c.life = mat.Life;
            c.type = type;
            c.A = cl.A;
            c.R = cl.R;
            c.G = cl.G;
            c.B = cl.B;
            return c;
        }
        public Cell GetCell(Int32 x, Int32 y, float R, float G, float B)
        {
            Material mat = Materials.Get(type);
            Cell c = new Cell();


            c.life = life;
            c.type = type;
            Color cl = Sample(Math.Abs(x), Math.Abs(y));
            c.A = cl.A;
            c.R = (byte)(cl.R*R);
            c.G = (byte)(cl.G*G);
            c.B = (byte)(cl.B*B);
            return c;
        }
        public Cell GetCell(Int32 x, Int32 y)
        {
            Material mat = Materials.Get(type);
            Cell c = new Cell();

            c.life = life;
            c.type = type;
            Color cl = Sample(x, y);
            c.A = cl.A;
            c.R = cl.R;
            c.G = cl.G;
            c.B = cl.B;
            return c;
        }
        static Random rng = new Random();
        public Cell GetCell(Int32 x, Int32 y, params Color[] colors)
        {
            Material mat = Materials.Get(type);
            Cell c = new Cell();


            c.life = life;
            c.type = type;

            Color cl = colors[rng.Next(0, colors.Length - 1)];

            c.A = cl.A;
            c.R = cl.R;
            c.G = cl.G;
            c.B = cl.B;
            return c;
        }
        /// <summary>
        /// Creates a cell for each color in the sampler
        /// </summary>
        /// <param name="world"></param>
        /// <param name="type"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        public void CreateFull(CellWorld world, Int32 sx, Int32 sy)
        {
            Material mat = Materials.Get(type);
            for (int x = 0; x != w; x++)
                for (int y = 0; y != h; y++)
                {
                   if (Count == 0)
                     return;

                    Cell c = new Cell();

                    c.life = life;
                    c.type = type;
                    Color cl = SampleUV(x,y);
                    c.A = cl.A;
                    c.R = cl.R;
                    c.G = cl.G;
                    c.B = cl.B;

                    if (c.A == 0 || (!replace && !world.IsEmpty(sx + x, sy + y)))
                        continue;

                    world.SetCell(sx+x, sy+y, c);
                    world.Wake(sx + x, sy + y);
                    if (Consume)
                        Count--;
                }

        }
        public static void Paint(CellWorld world, byte paint, int h, int w, int sx, int sy)
        {
            for (int x = 0; x != w; x++)
                for (int y = 0; y != h; y++)
                {


                    if (world.InBounds(sx + x, sy + y) && !world.IsEmpty(sx + x, sy + y))
                    {
                        Cell c = world.GetCell(sx + x, sy + y);
                        c.paint = paint;
                        world.SetCell(sx + x, sy + y, c);
                        world.Wake(sx + x, sy + y);
                    }


                }
        }
        public void Create(CellWorld world, int sx, int sy, int height, int width)
        {

            int center = (height / 2 + width / 2) / 2;
            int radw = width / 2;
            int radh = height / 2;
            Material mat = Materials.Get(type);
            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                {

                    if(Math.Sqrt(Math.Pow(x - radh, 2) + Math.Pow(y - radw, 2)) < center)
                        SetCell(mode, world, mat, Math.Max(0, sx + x), Math.Max(0, sy + y));
                }

        }
        public void Create(CellWorld world, int sx, int sy, int height, int width, FillMode mode)
        {
            FillMode o = Sampler.mode;
            Sampler.mode = mode;
            Create(world, sx, sy, height, width);
            Sampler.mode = o;
        }
        public void CreateRect(CellWorld world, Rectangle rect, FillMode mode)
        {
            FillMode o = Sampler.mode;
            Sampler.mode = mode;
            CreateRect(world, rect);
            Sampler.mode = o;
        }
        private void CreateRect(CellWorld world, Rectangle rect)
        {
            int sx = rect.X;
            int sy = rect.Y;
            int width = rect.Width;
            int height = rect.Height;
            Material mat = Materials.Get(type);
            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                {

                    SetCell(mode, world, mat, sx + x, sy + y);
                }
        }
        public static void ClearRect(CellWorld world, Rectangle rect)
        {
            int sx = rect.X;
            int sy = rect.Y;
            int width = rect.Width;
            int height = rect.Height;
            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                {

                    world.Clear(sx+x, sy+y);
                }
        }
        public static void ClearRect(CellWorld world, Rectangle rect, Cell c)
        {
            int sx = rect.X;
            int sy = rect.Y;
            int width = rect.Width;
            int height = rect.Height;
            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                {

                    world.SetCell(sx + x, sy + y, c);
                }
        }
        public static void MakeParticles(CellWorld world, int sx, int sy, int width, int height, float vmult = 0.5f)
        {
            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                {
                    Cell c = world.GetCell(sx + x, sy + y);
                    world.Clear(sx + x, sy + y);
                    if(c.type > 0)
                    {
                        float vx = (x - width / 2) * (vmult / 20) + Game.rng.NextFloat(-vmult, vmult);
                        float vy = (y - height / 2) * (vmult / 20) + Game.rng.NextFloat(-vmult, vmult);
                        ParticleEngine.AddParticleWithVelocity(c, sx + x, sy + y, vx,vy);
                    }
                }
        }
        public static void MakeParticlesCircle(CellWorld world, int sx, int sy, int radius, float vmult = 0.5f)
        {
            
            int cx = sx + radius;
            int cy = sy + radius;
            for (int x = 0; x != radius * 2 + 1; x++)
                for (int y = 0; y != radius * 2 + 1; y++)
                {
                    Cell c = world.GetCell(sx + x, sy + y);

   
                    int _x = x + sx;
                    int _y = y + sy;
                    if (c.type > 0)
                    {
                        bool inside = Math.Sqrt(Math.Pow(x - radius, 2) + Math.Pow(y - radius, 2)) < radius;
                        if (inside)
                        {
                            world.Clear(_x, _y);

                            float mag = (float)Math.Sqrt(Math.Pow((cx - _x), 2) + Math.Pow((cy - _y), 2));

                            float dx = -(cx - _x) / mag * (radius - mag);
                            float dy = -(cy - _y) / mag * (radius - mag);

                            float dmag = (float)Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

                            float vx = dx / dmag;
                            float vy = dy / dmag;

                            ParticleEngine.AddParticleWithVelocity(c, sx + x, sy + y, vx, vy);
                        }

                    }
                }
        }
        public static bool Dead = false;        
        public static void CollectCells(CellWorld world, int sx, int sy, int width, int height, ref Cell[,] cells, int body)
        {

            int collected = 0;
            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                {
                    Cell c = world.GetCell(sx + x, sy + y);
                    if (c.owner == (short)body)
                    {


                        world.Clear(sx + x, sy + y);
                        cells[x, y] = c;
                        collected++;
                        continue;
                    }

                     cells[x, y] = Cell.Zero;

                }
            if (collected == 0)
                Dead = true;
        }

        public static void InsertArray(CellWorld world, int sx, int sy, int width, int height, ref Cell[,] cells, int body)
        {
            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                {
                    if (cells[x, y].type > 0)
                    {
                        cells[x, y].owner = (short)body;
                        world.SetCell(sx + x, sy + y, cells[x, y]);
                    }

                }
        }

        public void Create(CellWorld world, Rectangle rect, FillMode mode)
        {
            Material mat = Materials.Get(type);
            int sx = rect.X;
            int sy = rect.Y;
            int width = rect.Width;
            int height = rect.Height;
            if(mode == FillMode.Hallow)
            {
                int thick = 1;

                Rectangle r1 = new Rectangle(sx, sy, width, thick);
                Rectangle r2 = new Rectangle(sx, sy + height - thick, width, thick);

                Rectangle r3 = new Rectangle(sx, sy + thick, thick, height - (thick * 2));
                Rectangle r4 = new Rectangle(sx + width - thick, sy + thick, thick, height - (thick * 2));

                CreateRect(world, r1);
                CreateRect(world, r2);
                CreateRect(world, r3);
                CreateRect(world, r4);
                return;
            }
            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                {

                    SetCell(mode, world, mat, sx + x, sy + y);
                }

        }
        public void SetCell(FillMode mode, CellWorld world, Material mat, int x, int y)
        {
            if(mode == FillMode.Fill || mode == FillMode.Replace)
            {
                if (Count == 0 && Consume)
                    return;

                Cell c = new Cell();

                c.life = life;
                c.type = type;
                Color cl = Sample(x, y);
                c.A = cl.A;
                c.R = cl.R;
                c.G = cl.G;
                c.B = cl.B;

                if (c.A == 0 || (mode != FillMode.Replace && !world.IsEmpty(x, y)))
                    return;

                world.SetCell(x, y, c);
                world.Wake(x, y);


                if (Consume)
                    Count--;

                return;
            }
            if(mode == FillMode.Erase)
            {
                if (Materials.Get(world.GetCell(x, y).type).Hardness > hardcap)
                    return;
                if (world.IsEmpty(x, y))
                    return;

                if (Consume)
                {
                    Sampler samp = GetSampler(world.GetCell(x, y).type);
                    samp.Count++;
                }

                world.Clear(x, y);
                world.Notify(x, y);

            }
        }
    }
}
