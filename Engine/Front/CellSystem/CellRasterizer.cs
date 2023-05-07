
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
                return (((x * 0x1f1f1f1f) ^ y) * 16807u) % max;
            };
        }

        public virtual GFX.Color RasterizeCell(Cell cell, int x, int y, uint sx, uint sy)
        {
            
            return GFX.Color.FromArgb(cell.A, cell.R, cell.G, cell.B);
        }
    }
}
