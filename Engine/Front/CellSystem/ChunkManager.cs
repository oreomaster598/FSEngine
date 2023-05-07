using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.CellSystem
{
    /// <summary>
    /// Gets chunks that aren't in use, or generates them if the chunk doesn't exist.
    /// </summary>
    public interface IChunkManager
    {
        CellChunk GetChunk(Int32 x, Int32 y);
        (CellChunk, bool) Generate(Int32 x, Int32 y);
    }
}
