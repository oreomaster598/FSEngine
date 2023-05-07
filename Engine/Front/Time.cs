using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine
{
    public static class Time
    {
        public static Single target = 16.66666f;
        public static Single fps;
        public static Single delta = 1;
        public static Single current_mspf = 0;

        public static int t_frames = 1;
        public static long frames = 1;
        public static Stopwatch sw = Stopwatch.StartNew();
        static long start;

        public static void SetFrequency(float fps) => target = 1000f / fps;
        public static bool Tick()
        {
            long ms = sw.Elapsed.Milliseconds;
            float mspf = ms - start;

            if (mspf >= target)
            {
                fps = 1000.0f / mspf;
                current_mspf = mspf;
                delta = 1.0f / (target / mspf);
                start = ms;
                return true;
            }
            return false;
        }

    }
}
