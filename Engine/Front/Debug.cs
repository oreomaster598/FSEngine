using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine
{
    public static class Debug
    {
        static StringBuilder writer = new StringBuilder();
        #if DEBUG
                const bool log = true;
        #else
                const bool log = false;
        #endif
        const int max_length = 10000;//[CallerMemberName]
        public static void LogError(string error, [CallerMemberName] string sender = "")
        {
            //writer.AppendLine($"{DateTime.Now.ToLongDateString()}, {DateTime.Now.ToLongTimeString()}");
            writer.AppendLine($"[{sender.ToUpper()}] {error}");
            SendColor(Color.Red);
            Console.WriteLine($"[{sender.ToUpper()}] {error}");
        }
        public static void LogWarning(string error, [CallerMemberName] string sender = "")
        {
            //writer.AppendLine($"{DateTime.Now.ToLongDateString()}, {DateTime.Now.ToLongTimeString()}");
            writer.AppendLine($"[{sender.ToUpper()}] {error}");
            SendColor(Color.Yellow);
            Console.WriteLine($"[{sender.ToUpper()}] {error}");
        }
        public static void Log(string error, [CallerMemberName] string sender = "")
        {
            //writer.AppendLine($"{DateTime.Now.ToLongDateString()}, {DateTime.Now.ToLongTimeString()}");
            writer.AppendLine($"[{sender.ToUpper()}] {error}");
            SendColor(Color.Aqua);
            Console.WriteLine($"[{sender.ToUpper()}] {error}");
        }
        public static void Log(string error, Color color, [CallerMemberName] string sender = "")
        {
            //writer.AppendLine($"{DateTime.Now.ToLongDateString()}, {DateTime.Now.ToLongTimeString()}");
            writer.AppendLine($"[{sender.ToUpper()}] {error}");
            SendColor(color);
            Console.WriteLine($"[{sender.ToUpper()}] {error}");
        }
        public static void ExportLog()
        {
            string log = writer.ToString();
            string date = $"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}_{DateTime.Now.Hour}-{DateTime.Now.Minute}";

            File.WriteAllText($"log/{date}.log", log);
        }

        internal static void SendColor(Color c)
        {
            Console.Write($"\x1b[38;2;{c.R};{c.G};{c.B}m");
        }
    }
}
