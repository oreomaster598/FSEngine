using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine
{
    public class Statistics
    {
        public int Shaders, Textures, Sounds, Models, Files;
        public int b2dBodies, b2dJoints, b2dTriangles;

        public int rendered_chunks;
        public double render_ms, step_cells_ms, step_world_ms;
    }
}
