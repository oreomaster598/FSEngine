using FSEngine.CellSystem;
using FSEngine.Concurrency;
using FSEngine.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine 
{
    public class ChunkDictionary
    {
        /// <summary>
        /// mutable array of chunks
        /// </summary>
        public CellChunk[,] chunks;
        /// <summary>
        /// immutable array of chunks
        /// </summary>
        public CellChunk[,] ichunks;

        public List<CellChunk> ToKill = new List<CellChunk>(100);

        int origin_x = 0, origin_y = 0;
        int sx, sy;
        int buffer;
        public ChunkDictionary(int width, int height, int buffer)
        {
            this.buffer = buffer;
            sx = width + (buffer * 2);
            sy = height + (buffer * 2);
            chunks = new CellChunk[sx, sy]; 
            ichunks = new CellChunk[sx, sy];
        }
        
        /// <summary>
        /// Forces a chunk into the world.
        /// </summary>
        /// <param name="chunk"></param>
        public void Push(CellChunk chunk)
        {
            chunk.save = false;
            if(chunk.filledcells > 0)
            {
                State.SaveChunk(chunk, ChunkCache.cache);
            }
            if(InBounds(chunk.x, chunk.y))
            {
                chunk.has_file = true;
                this[chunk.x, chunk.y] = chunk;
            }
        }
        ///Waits until CleanUp to kill chunk
        public void LazyKill(int x, int y)
        {
            CellChunk c = chunks[x - origin_x, y - origin_y];
            chunks[x - origin_x, y - origin_y] = null;
            if (c != null && !ToKill.Contains(c))
            {
                ToKill.Add(c);
            }

        }

        public void Cycle(int _x, int _y)
        {

            foreach (CellChunk c in ToKill)
            {
                if (c == null)
                    continue;

                if (c.has_file)
                {
                    File.Delete($"{ChunkCache.cache}/chunk-{c.x},{c.y}.dat");
                }


            }

            ToKill.Clear();

            if (origin_x == _x && origin_y == _y)
                return;

            int dx = _x - origin_x;
            int dy = _y - origin_y;



            Array.Copy(chunks, ichunks, chunks.Length);
            Array.Clear(chunks, 0, chunks.Length);

            for (int x = 0; x < sx; x++)
            {
                for (int y = 0; y < sy; y++)
                {
                    int mx = x - dx;
                    int my = y - dy;
                    int tx = mx + _x;
                    int ty = my + _y;
                    bool InBounds = my >= 0 && mx >= 0 && my < sy && mx < sx;




                    if (ichunks[x, y] == null && InBounds)
                    {

                        if (File.Exists($"{ChunkCache.cache}/chunk-{(int)mx + _x},{(int)my + _y}.dat"))
                        {
                            chunks[mx, my] = State.LoadChunk(mx + _x, my + _y, ChunkCache.cache);
                            continue;
                        }
                    }



                    if (InBounds)
                    {
                        if (ichunks[x, y] != null && ichunks[x, y].filledcells == 0)
                        {
                            if (ichunks[x, y].has_file)
                            {
                                File.Delete($"{ChunkCache.cache}/chunk-{ichunks[x, y].x},{ichunks[x, y].y}.dat");
                                chunks[mx, my] = null;
                            }
                        }
                        else
                        {
                            chunks[mx, my] = ichunks[x, y];
                            if(chunks[mx, my] != null)
                            {
                                chunks[mx, my].rendered = false;
                            }

                            if (chunks[mx, my] != null && (chunks[mx, my].x != tx || chunks[mx, my].y != ty))
                            {
                                chunks[mx, my] = null;
                                Debug.LogWarning("Duplicate Chunk Reference.", "ChunkDictionary");
                            }
                        }
 
                    }
                    else if(ichunks[x, y] != null)// save the chunks that are out of bounds
                    {
                        if (ichunks[x, y].filledcells == 0)
                        {
                            if(ichunks[x, y].has_file)
                            {
                                File.Delete($"{ChunkCache.cache}/chunk-{ichunks[x, y].x},{ichunks[x, y].y}.dat");
                            }
                        }
                        else if(ichunks[x, y].save)
                        {
                            State.SaveChunk(ichunks[x, y], ChunkCache.cache);
                        }
                    }

                }
            }


            origin_x = _x;
            origin_y = _y;
        }

        public bool InBounds(int x, int y)
        {
            return x >= origin_x && y >= origin_y && y < (sy + origin_y) && x < (sx + origin_x);
        }
        public bool Simulate(int x, int y)
        {
            return x >= origin_x + 2 && y >= origin_y + 2 && y < (sy + origin_y - 2) && x < (sx + origin_x - 2);
        }
        public bool InFrame(int x, int y)
        {
            return x >= origin_x + buffer - 1 && y >= origin_y + buffer - 1 && y < (sy + origin_y - buffer + 1) && x < (sx + origin_x - buffer + 1);
        }
        public void Add( Vector2 key, CellChunk chunk)
        {
            chunks[(int)key.X - origin_x, (int)key.Y - origin_y] = chunk;
        }
        public void Remove(Vector2 key)
        {
            chunks[(int)key.X - origin_x, (int)key.Y - origin_y] = null;
        }

        public bool ContainsKey(Vector2 key)
        {
            if (!InBounds((int)key.X, (int)key.Y))
                return false;
            return chunks[(int)key.X - origin_x, (int)key.Y - origin_y] != null;
        }
        public void Add(CellChunk chunk)
        {
            chunks[chunk.x - origin_x, chunk.y - origin_y] = chunk;
        }
        public void Remove(int x, int y)
        {
            chunks[x - origin_x, y - origin_y] = null;
        }

        public bool ContainsKey(int x, int y)
        {
            return chunks[x - origin_x, y - origin_y] != null;
        }

        public CellChunk this[Vector2 key]
        {
            get
            {
                return chunks[(int)key.X - origin_x, (int)key.Y - origin_y];
            }
            set
            {
                chunks[(int)key.X - origin_x, (int)key.Y - origin_y] = value;
            }
        }
        public CellChunk this[int x, int y]
        {
            get
            {
                return chunks[x - origin_x, y - origin_y];
            }
            set
            {
                chunks[x - origin_x, y - origin_y] = value;
            }
        }
    }
}
