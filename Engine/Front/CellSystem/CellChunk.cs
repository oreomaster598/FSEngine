using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.CellSystem
{
    
    public unsafe class CellChunk
    {

        internal Cell[,] cells;
        public Int32 x, y;
        public Boolean Asleep;
        public byte maxX, maxY, minX, minY;
        public byte maxXw, maxYw, minXw, minYw;
        public short filledcells = 0;
        public short biome = 0;
        public ushort updatedcells = 0;

        public bool save = false;
        public bool has_file = false;

        public bool rendered = false;

        public CellChunk(Int32 x, Int32 y, bool init = false)
        {
            this.x = x;
            this.y = y;
            minX = 0; minXw = 0; maxXw = 0;
            minY = 0; minYw = 0; maxYw = 0;
            if(init)
            {
                KeepAlive(2, 2);
                KeepAlive(CellWorld.chunk_s - 3, CellWorld.chunk_s - 3);
            }
            cells = new Cell[CellWorld.chunk_s, CellWorld.chunk_s];
            
        }

        [BlazePreJIT]
        public void KeepAlive(int x, int y)
        {
            minXw = (byte)Utils.Clamp(Math.Min(x - 2, minXw), 0, CellWorld.chunk_s);
            minYw = (byte)Utils.Clamp(Math.Min(y - 2, minYw), 0, CellWorld.chunk_s);
            maxXw = (byte)Utils.Clamp(Math.Max(x + 2, maxXw), 0, CellWorld.chunk_s);
            maxYw = (byte)Utils.Clamp(Math.Max(y + 2, maxYw), 0, CellWorld.chunk_s);

        }

        [BlazePreJIT]
        public void UpdateRect()
        {
            // Update current; reset working

            minX = minXw; minXw = (byte)CellWorld.chunk_s;
            minY = minYw; minYw = (byte)CellWorld.chunk_s;
            maxX = maxXw; maxXw = 0;
            maxY = maxYw; maxYw = 0;

            if (minX > maxX)
                minX = 0; 
            if (minY > maxY)
                minY = 0;
        }

        [BlazePreJIT]
        public bool IsEmpty(Int32 x, Int32 y) 
        {
            return cells[x, y].type == 0;
        }

        [BlazePreJIT]
        public Cell GetCell(Int32 x, Int32 y)
        {
            return cells[x, y];
        }

        [BlazePreJIT]
        public void SetCell(Int32 x, Int32 y, Cell cell)
        {
            if (cell.type == 0 && cells[x, y].type > 0)
                filledcells--;

            if (cell.type > 0 && cells[x, y].type == 0)
                filledcells++;


            cells[x, y] = cell;
            KeepAlive(x, y);
            save = true;
        }

        [BlazePreJIT]
        public void Clear(Int32 x, Int32 y) 
        {
            SetCell(x, y, Cell.Zero);
        }

        [BlazePreJIT]
        public void SetCellDead(Int32 x, Int32 y, Cell cell)
        {
            cells[x, y] = cell;
            if (cell.type > 0 && cells[x, y].type == 0)
                filledcells++;
            else if (filledcells > 0 && cells[x, y].type != 0)
                filledcells--; 
            save = true;
        }

        [BlazePreJIT]
        public bool InBounds(Int32 x, Int32 y)
        {
            return x >= this.x && x < this.x + CellWorld.chunk_s && y >= this.y && y < this.y + CellWorld.chunk_s;
        }
    }
}
