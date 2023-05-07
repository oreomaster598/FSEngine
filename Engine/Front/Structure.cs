using FSEngine.CellSystem;
using FSEngine.Concurrency;
using FSEngine.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine
{
    public enum StructureID : int
    {
        Cabin,
        Debug_Center
    }
    public static class Structures
    {
        public static List<Structure> structs = new List<Structure>();
        
        public static void Load(string csv)
        {
            CSV data = CSVLoader.Load(csv);
            for (int i = 0; i < data.rows; i++)
            {
                string path = data.data[i, 0];
                if (path.StartsWith("./"))
                    path = path.Replace("./", Path.GetDirectoryName(csv) + "/");
                LoadStructure(path, bool.Parse(data.data[i, 1]));
            }
        }
        public static void LoadStructure(string file, bool gfx)
        {
            //using (Bitmap bmp = new Bitmap(file))
            Structure s = Structure.Load(file, gfx);
            if (s == null)
                Console.WriteLine($"Failed To Load Structure '{Path.GetFileNameWithoutExtension(file)}'") ;
            else
                structs.Add(s);
        }
    }
    public class Structure
    {
        internal Int16[,] cells;
        internal TSBitmap gfx;
        public int width, height;
        public static Dictionary<Color, MaterialType> mappings = new Dictionary<Color, MaterialType>() 
        {
            { Color.FromArgb(255,0,0,255), MaterialType.Water},
            { Color.FromArgb(255,0,255,0), MaterialType.Acid},
            { Color.FromArgb(255,128,128,128), MaterialType.Rock},
            { Color.FromArgb(255,192,192,192), MaterialType.Iron},
            { Color.FromArgb(255,85,50,25), MaterialType.Wood},
            { Color.FromArgb(255,255,255,255), MaterialType.Salt},
            { Color.FromArgb(255,115,192,255), MaterialType.Glass},
            { Color.FromArgb(255,255,90,0), MaterialType.Lava},
        };
        public Structure(int width, int height)
        {
            this.width = width;
            this.height = height;
            cells = new Int16[width, height];
        }
        /// <summary>
        /// WIP
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Structure FromBitmap(Bitmap bmp)
        {
            Structure s = new Structure(bmp.Width, bmp.Height);
            for (int x = 0; x < s.width; x++)
            {
                for (int y = 0; y < s.height; y++)
                {
                    Color c = bmp.GetPixel(x, y);
                    if (mappings.ContainsKey(c))
                    {
                        s.cells[x, y] = (short)mappings[bmp.GetPixel(x, y)];
                    }
                }
            }
            return s;
        }
        public static Structure Load(string file, bool gfx)
        {
            Bitmap bmp = new Bitmap(file);
            Structure s = new Structure(bmp.Width, bmp.Height);
            if (gfx)
            {
                if (File.Exists(file.Replace(".png", "_gfx.png")))
                {
                    s.gfx = new TSBitmap(file.Replace(".png", "_gfx.png"));
                    Console.WriteLine($"Found Graphic for {Path.GetFileNameWithoutExtension(file)}");
                }
                else
                    return null;
            }

                

            for (int x = 0; x < s.width; x++)
            {
                for (int y = 0; y < s.height; y++)
                {
                    Color c = bmp.GetPixel(x, y);
                    if (mappings.ContainsKey(c))
                    {
                        s.cells[x, y] = (short)mappings[bmp.GetPixel(x, y)];
                    }
                }
            }
            return s;
        }
        public void Create(CellWorld world, Vector2 pos)
        {
            if(gfx != null)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (cells[x, y] == 0)
                            continue;
                        Cell c = Sampler.GetSampler(cells[x, y]).GetCell(x, y);
                        Color clr = gfx.GetPixel((uint)x, (uint)y, PixelFormat.ABGR);
                        c.A = clr.A;
                        c.R = clr.R;
                        c.G = clr.G;
                        c.B = clr.B;
                        world.SetCell((int)pos.X + x - width, (int)pos.Y + y - height, c);
                    }
                }
                return;
            }
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cells[x, y] == 0)
                        continue;
                    Cell c = Sampler.GetSampler(cells[x,y]).GetCell(x,y);
                    world.SetCell((int)pos.X+x-width, (int)pos.Y+y-height, c);
                }
            }
        }
        
    }
}
