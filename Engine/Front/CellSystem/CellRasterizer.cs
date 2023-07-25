
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.CellSystem
{
    public class CellRasterizer
    {
        public virtual void Init()
        {
            
        }
        public int FastRand(int x, int y)
        {
            unchecked 
            { 
                return (x ^ y) * 16807;
            };
        }
        public uint FastRand(uint x, uint y, uint max)
        {
            unchecked
            {
                return (((x * 0x1f1f1f1f) ^ y) * 16807u ^ seed) % max;
            };
        }
        public uint FastRand(uint x, uint y, uint max, uint magic)
        {
            unchecked
            {
                return (((x * 0x1f1f1f1f) ^ y ) * 16807u ^ magic) % max;
            };
        }
        public uint seed = 5243;

        public float FastRand()
        {
            unchecked
            {
                return (float)((seed *= 16807u) % 100) / 100f;
            };
        }
        public uint FastRandi()
        {
            unchecked
            {
                return seed *= 16807u;
            };
        }
        public virtual GFX.Color RasterizeCell(Cell cell, int x, int y, uint sx, uint sy)
        {
            
            return GFX.Color.FromArgb(cell.A, cell.R, cell.G, cell.B);
        }
    }
}
