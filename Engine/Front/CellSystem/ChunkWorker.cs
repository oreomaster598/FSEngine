using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Threading;
using FSEngine.Front.CellSystem.Events;
using Vector2 = FSEngine.Vector2FI;
namespace FSEngine.CellSystem
{
    public unsafe class ChunkWorker
    {
        public CellChunk chunk;
        public CellWorld world;
        public Material material;
        Int16 Skipped;
        Int16 Looped;

        public static TSRandom rng = new TSRandom();

        public static int Randomness = 5;
        public double rn = 0;
        public ChunkWorker(CellChunk chunk, CellWorld world)
        {
            this.chunk = chunk;
            this.world = world;
        }
        public void Swap(int x1, int y1, int x2, int y2)
        {
            Cell c1 = world.GetCell(x1, y1);
            Cell c2 = world.GetCell(x2, y2);
            if (Materials.Get(c2.type).Process == 0)
                return;
            world.SetCell(x1, y1, c2);
            world.SetCell(x2, y2, c1);
        }
        public void Corrode(int x, int y)
        {

            Material mat;
            if (!world.IsEmpty(x + 1, y))
            {
                mat = Materials.Get(world.GetCell(x + 1, y).type);
                if (mat.CorrosionResist < material.Corrosive)
                {
                    world.Clear(x + 1, y);
                }
            }
            if (!world.IsEmpty(x - 1, y))
            {
                mat = Materials.Get(world.GetCell(x - 1, y).type);
                if (mat.CorrosionResist < material.Corrosive)
                {
                    world.Clear(x - 1, y);
                }
            }
            if (!world.IsEmpty(x, y + 1))
            {
                mat = Materials.Get(world.GetCell(x, y + 1).type);
                if (mat.CorrosionResist < material.Corrosive)
                {
                    world.Clear(x, y + 1);
                }
            }
            if (!world.IsEmpty(x, y - 1))
            {
                mat = Materials.Get(world.GetCell(x, y - 1).type);
                if (mat.CorrosionResist < material.Corrosive)
                {
                    world.Clear(x, y - 1);
                }
            }
        }
        public bool IsTouching(int x, int y, short id)
        {
            bool b = false;
            if (!world.IsEmpty(x + 1, y))
            {
                b = b || world.GetCell(x + 1, y).type == id;
            }
            if (!world.IsEmpty(x - 1, y))
            {
                b = b || world.GetCell(x + 1, y).type == id;
            }
            if (!world.IsEmpty(x, y + 1))
            {
                b = b || world.GetCell(x + 1, y).type == id;
            }
            if (!world.IsEmpty(x, y - 1))
            {
                b = b || world.GetCell(x + 1, y).type == id;
            }
            return b;
        }
        public void Update()
        {
            if (chunk.x < 1 || chunk.y < 1)
                return;
            //try
            {
                if (chunk.filledcells == 0)
                {
                    world.cache.chunks.LazyKill(chunk.x, chunk.y);
                    return;
                }
                chunk.updatedcells = 0;
                for (Int32 x = chunk.minX; x < chunk.maxX; x++)
                    for (Int32 y = chunk.minY; y < chunk.maxY; y++)
                    {
                        
                        Looped++;

                        

                        rn = rng.NextDouble();
                        int rand = (int)((Randomness + 1) * rn);
        
                        Cell c = chunk.GetCell(x, y);

                        if (c.type == 0)
                        {
                            Skipped++;
                            continue;
                        }
                        material = Materials.Get(c.type);
                        if(!(c.frame != world.frame /*&& material.Process != 0*/))
                        {
                            Skipped++;
                            continue;
                        }

                        CellWorld.CellsUpdated++;

                        #region Update
                        Int32 _x = x + (chunk.x * CellWorld.chunk_s);
                        Int32 _y = y + (chunk.y * CellWorld.chunk_s);

                        bool cease = UpdateCell(c, x, y, ref _x, ref _y, rn);
                        //c->frame = (UInt32)world.frame;


                        if (_x < 0) _x = 0;
                        if (_y < 0) _y = 0;

                        bool Moved = !(x == _x % CellWorld.chunk_s && y == _y % CellWorld.chunk_s);
                        if((int)(2 * rn) == 0)
                        {
                            if(Moved)
                            {
                                int ping_x = CellWorld.chunk_s * chunk.x + x, ping_y = CellWorld.chunk_s * chunk.y + y;

                                world.Notify(ping_x + 1, ping_y);
                                world.Notify(ping_x - 1, ping_y);
                                world.Notify(ping_x, ping_y + 1);
                                world.Notify(ping_x, ping_y - 1);
                            }

                            chunk.updatedcells++;
                            continue;
                        }
                        if(cease)
                        {
                            int ping_x = CellWorld.chunk_s * chunk.x + x, ping_y = CellWorld.chunk_s * chunk.y + y;

                            world.Notify(ping_x + 1, ping_y);
                            world.Notify(ping_x - 1, ping_y);
                            world.Notify(ping_x, ping_y + 1);
                            world.Notify(ping_x, ping_y - 1);
                            chunk.updatedcells++;
                            continue;
                        }
                        if (Moved)
                        {

                            //PING
                            int ping_x = CellWorld.chunk_s * chunk.x + x, ping_y = CellWorld.chunk_s * chunk.y + y;

                            world.Notify(ping_x + 1, ping_y);
                            world.Notify(ping_x - 1, ping_y);
                            world.Notify(ping_x, ping_y + 1);
                            world.Notify(ping_x, ping_y - 1);
                            world.MoveCell(chunk, x, y, _x, _y);
 
                            chunk.updatedcells++;
                        }
                        else
                        {

                            Skipped++;
                        }


                        #endregion

                    }

                if (Skipped == Looped)
                {
                    chunk.Asleep = true;
                }

            }
            //catch(Exception e)
            //{
             //   Console.WriteLine(e.Message);
            //}
            // Console.WriteLine(new Vector2(chunk.x, chunk.y) + ":"+chunk.updatedcells);
        }

        /// <summary>
        /// Update Cell
        /// </summary>
        /// <param name="c">Cell to Update</param>
        /// <param name="x">X Position in Chunk</param>
        /// <param name="y">Y Position in Chunk</param>
        /// <param name="_x">X Position in World</param>
        /// <param name="_y">Y Position in World</param>
        /// <returns></returns>
        public virtual bool UpdateCell(Cell c, int x, int y, ref int _x, ref int _y, double rn)
        {
            
            return true;
        }
    }
}
