using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine
{
    public static class Mouse
    {
        public static int Delta;
        public static Vector2 CursorPos;
        public static bool Middle;
        public static bool LeftDown;
        public static bool RightDown;

        internal static bool LastLeft;
        internal static bool LastRight;

        public static bool LeftUp;
        public static bool RightUp;
    }
}
