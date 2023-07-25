using Microsoft.Win32.SafeHandles;
using FSEngine.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows;
using FSEngine.Audio;
using System.Diagnostics;
using FSEngine.Concurrency;
using System.Reflection;

namespace FSEngine
{
    public class Program
    {
        public static Window window;
        public static Assembly game_asm;
        public const int MaxThreads = 8;
        static async Task Main(string[] args)
        {            
            IntPtr handle = Kernel.GetStdHandle(-11);
            Kernel.SetConsoleMode(handle, ConsoleFlag.VIRTUAL_TERMINAL_PROCESSING | ConsoleFlag.ENABLE_WRAP_AT_EOL_OUTPUT | ConsoleFlag.ENABLE_PROCESSED_OUTPUT);

            Debug.Log("Enabled Virtual Terminal Processing", "PROGRAM");

            Debug.Log("Starting Pre-JIT", System.Drawing.Color.OrangeRed, "Blazing");
            Blazing.JITStatus status = Blazing.WarmUp();
            Debug.Log(status.ShortString(), System.Drawing.Color.OrangeRed, "Blazing");



            ThreadPool.SetMaxThreads(MaxThreads, MaxThreads);
            Debug.Log($"Set Max Threads to {MaxThreads}", "PROGRAM");


            Blazing.StartInternalTimer();

            window = new Window(typeof(Tests.TestGame));

            Debug.Log("Created Window", "PROGRAM");

            SoundManager.InitializeOpenAL();

            window.Run();

            SoundManager.CloseOpenAL();


            Debug.ExportLog();
            Console.ReadLine();
        }
        

    }
}
