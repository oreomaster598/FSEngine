using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using FSEngine.IO;
using System.IO;
using FSEngine.Concurrency;

namespace FSEngine.CellSystem
{
    public class ChunkCache
    {
        public static string cache = @"Saves\Save0\Chunks";



        public ChunkDictionary chunks = new ChunkDictionary(12, 8, 5);

     
        public CellChunk this[Vector2 pos]
        {
            get
            {
                return chunks[pos];
            }
            set
            {
                chunks[pos] = value;
            }
        }


    }
}
