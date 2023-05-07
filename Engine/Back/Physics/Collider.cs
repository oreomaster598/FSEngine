using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine
{
    
    public static class WorldCollisionMesh
    {
        internal static bool[,] CollisionMesh;

        public static bool Colliding(int x, int y)
        {
            return CollisionMesh[x - (int)Window.game.cellworld.PixelOffset.X, y - (int)Window.game.cellworld.PixelOffset.Y];
        }
        internal static void SetColliding(int x, int y, bool colliding)
        {
            CollisionMesh[x - (int)Window.game.cellworld.PixelOffset.X, y - (int)Window.game.cellworld.PixelOffset.Y] = colliding;
        }
    }
}
