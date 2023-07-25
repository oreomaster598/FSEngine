using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.CellSystem
{
    public class WeakCollider
    {

        public int width, height;
        public int x, y;
        int cx, cy;
        bool init = true;
        public bool colliding = false;
        public void CreateBarrier(CellWorld world, int sx, int sy, int height, int width)
        {

            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                {
                    if (world.GetCell(sx + x, sy + y).type == 0)
                        world.SetCell(sx + x, sy + y, new Cell(-1));

                }

        }
        public void RemoveBarrier(CellWorld world, int sx, int sy, int height, int width)
        {
            colliding = false;
            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                { Cell c = world.GetCell(sx + x, sy + y);

                    if (c.type == -1)
                        world.Clear(sx + x, sy + y);
                    else if(c.type != 0)
                        colliding = true;
                }

        }

        public void Tick(CellWorld world)
        {



            if (init)
            {
                init = false;
            }
            else
            {
                RemoveBarrier(world, cx-1, cy-1, height+2, width+2);
                //Sampler.MakeParticles(world, x, y, width, height);
            }

            CreateBarrier(world, x-1, y-1, height+2, width+2);
            cx = x;
            cy = y;


        }
    }
}
