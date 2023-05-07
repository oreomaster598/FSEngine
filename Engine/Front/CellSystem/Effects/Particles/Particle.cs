using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.CellSystem.Effects.Particles
{
    public struct Particle
    {
        public bool bloom;
        public float x;
        public float y;
        public float vel_x;
        public float vel_y;
        public float drag;
        public int life;

        public Cell cell;
        public Particle(Boolean bloom, Cell cell, Int32 x, Int32 y, float drag, Int32 life)
        {
            this.bloom = bloom;
            this.x = x;
            this.y = y;
            this.vel_x = 0;
            this.vel_y = 0;
            this.drag = drag;
            this.life = life;
            this.cell = cell;
        }

        public Particle Clone()
        {
            return (Particle)MemberwiseClone();
        }
    }
}
