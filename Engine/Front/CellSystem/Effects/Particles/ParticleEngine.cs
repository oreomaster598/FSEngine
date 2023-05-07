using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.CellSystem.Effects.Particles
{
    public struct Emitter
    {
        public Particle partical;
        public int numparts;
        public int life;

        public Emitter(Particle partical, Int32 numparts, Int32 life = 1)
        {
            this.partical = partical;
            this.numparts = numparts;
            this.life = life;
        }
    }
    public static class ParticleEngine
    {

        public static List<Particle> Particles = new List<Particle>();        
        public static int MaxParticles = 2500;
        public static void AddParticle(Particle particle)
        {
            if (Particles.Count == MaxParticles)
                Particles.RemoveAt(0);
            Particles.Add(particle);
        }

        public static void AddParticle(Cell c, int x, int y)
        {
            if (Particles.Count == MaxParticles)
                Particles.RemoveAt(0);
            Particles.Add(new Particle() { x = x, y = y, life = -1, cell = c });
        }
        public static void AddParticleWithVelocity(Cell c, int x, int y)
        {
            if (Particles.Count == MaxParticles)
                Particles.RemoveAt(0);
            float vx = Game.rng.NextFloat(-0.5f, 0.5f);
            float vy = Game.rng.NextFloat(-0.5f, 0.5f);
            Particles.Add(new Particle() { x = x, y = y, life = -1, cell = c, vel_x = vx, vel_y = vy, drag = 0.025f });
        }
        public static void AddParticleWithVelocity(Cell c, int x, int y, float vx, float vy)
        {
            if (Particles.Count == MaxParticles)
                Particles.RemoveAt(0);
            Particles.Add(new Particle() { x = x, y = y, life = -1, cell = c, vel_x = vx, vel_y = vy, drag = 0.025f });
        }
        public static void NewFrame(CellWorld cellworld)
        {
            for (int i = 0; i < Particles.Count; i++)
            {
                
                Particle p = Particles[i];
                if (!p.Equals(default(Particle)))
                {
                    p.life--;
                    p.x += p.vel_x;
                    p.y += p.vel_y;

                    p.vel_x = Utils.Interpolate(0, p.vel_x, 1f / p.drag);
                    p.vel_y = Utils.Interpolate(0, p.vel_y, 1f / p.drag);

                    if(Math.Abs(p.vel_x) <= 0.25f && Math.Abs(p.vel_y) <= 0.25f)
                    {
                        p.life = 0;
                    }

                    Vector2 sp = cellworld.ToScreenPos(new Vector2((uint)p.x, (uint)p.y));
                    if(cellworld.front_buffer.IsTranlucentPixel((uint)sp.X, (uint)sp.Y))
                    cellworld.front_buffer.SetPixelSafe((uint)sp.X, (uint)sp.Y, GFX.Color.FromArgb(p.cell.A, p.cell.R, p.cell.G, p.cell.B));
                    //if(p.bloom)
                    // CellWorld.lights.SetPixelSafe((uint)sp.X, (uint)sp.Y, GFX.Color.FromArgb(255, p.cell.R, p.cell.G, p.cell.B));
                }
                Particles[i] = p;
                if (p.life == 0)
                {
                    int n = 0;
                    while (n < 200 && !cellworld.IsEmpty((int)(p.x), (int)(p.y)))
                    {
                        n++;
                        p.y--;
                    }
                    if(n < 200)
                    cellworld.SetCell((int)(p.x), (int)(p.y), p.cell);
                    /*else
                    {
                        int x = (int)p.x - 5;
                        int y = (int)p.y - 5;

                        for (int _i = -15; _i < 16; _i++)
                        {
                            for (int _j = -15; _j < 16; _j++)
                            {
                                if (cellworld.IsEmpty(x + _i, y + _j))
                                {
                                    cellworld.SetCell(x + _i, y + _j, p.cell);
                                    _i = 100;
                                    break;
                                }
                            }

                        }
                    }*/
                    Particles.RemoveAt(i);
                }

            }
        }

       
    }
}
