using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Numerics;
using OpenTK.Input;

namespace FSEngine
{

    public static class Keyboard
    {


        public static KeyboardState state;


        public static bool IsKeyDown(Key key) => state.IsKeyDown(key);
    }
}
