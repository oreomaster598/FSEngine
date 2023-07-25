using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System;



namespace FSEngine
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
    public class BlazeNoPreJITAttribute : Attribute
    {
        public BlazeNoPreJITAttribute()
        {

        }
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
    public class BlazePreJITAttribute : Attribute
    {
        public BlazePreJITAttribute()
        {
        }
    }

    public static class Blazing
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = false)]
        private static extern bool QueryPerformanceFrequency(out long lpPerformanceFreq);

        static double clock_freq = 0l;
        static long blaze_start = 0l;
        static long timer_start = 0l;
    
        public static long ElapsedMilliseconds
        {
            get
            {
                QueryPerformanceCounter(out long now);
                return (long)((now - blaze_start) / clock_freq);
            }
        }

        public struct JITStatus
        {
            public int compiled_types, compiled_methods, ignored_types, ignored_methods, compiled_assemblies;
            public string LongString()
            {
                return $"Compiled {compiled_methods} methods in {compiled_types} types and ignored {ignored_methods} methods in {ignored_types} types in {compiled_assemblies} assemblies";
            }
            public string ShortString()
            {
                return $"Pre-JIT {compiled_methods} Methods";
            }
        }
        public static JITStatus WarmUpAssembly(Assembly assembly, bool safe = true)
        {
            JITStatus status = new JITStatus();

            WarmUpAssembly(assembly, ref status, safe);

            return status;
        }
        public static void WarmUpAssembly(Assembly assembly, ref JITStatus status, bool safe = true)
        {
            Process p = Process.GetCurrentProcess();

            Type[] types = assembly.GetTypes();

            int m = status.ignored_methods;
            for (int i = 0; i < types.Length; i++)
            {
                bool jit_type = !CheckAttribute<BlazeNoPreJITAttribute>(types[i]);
                bool jit_all = false;

                if (safe && jit_type)
                    jit_all = CheckAttribute<BlazePreJITAttribute>(types[i]);

                if (jit_type)
                {
                    status.compiled_types++;
                    MethodInfo[] methods = types[i].GetMethods(
                            BindingFlags.DeclaredOnly |
                            BindingFlags.NonPublic |
                            BindingFlags.Public |
                            BindingFlags.Instance |
                            BindingFlags.Static
                            );



                    for (int j = 0; j < methods.Length; j++)
                    {
                        bool jit_safe = !methods[j].IsGenericMethod && !methods[j].ContainsGenericParameters &&
                                        !methods[j].IsGenericMethodDefinition && !methods[j].IsAbstract;

                        bool jit = !CheckAttribute<BlazeNoPreJITAttribute>(methods[j]);

                        if (jit && safe)
                        {
                            jit &= CheckAttribute<BlazePreJITAttribute>(methods[j]);
                            jit |= jit_all;
                        }


                        if (jit && jit_safe)
                        {
                            RuntimeHelpers.PrepareMethod(methods[j].MethodHandle);
                            status.compiled_methods++;
                        }
                        else
                        {
                            status.ignored_methods++;
                        }
                    }
                }
                if (CheckAttribute<BlazeNoPreJITAttribute>(types[i]) || m != status.ignored_methods)
                {
                    status.ignored_types++;
                }

            }
            status.compiled_assemblies++;
        }
        public static JITStatus WarmUpApp(bool safe = true)
        {
            JITStatus status = new JITStatus();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                WarmUpAssembly(assemblies[i], ref status, safe);
            }
            return status;
        }
        public static JITStatus WarmUp(bool safe = true)
        {
            JITStatus status = new JITStatus();

            WarmUpAssembly(Assembly.GetExecutingAssembly(), ref status, safe);

            return status;
        }
        private static bool CheckAttribute<T>(MethodInfo methodInfo) where T : Attribute
        {
            object[] attributes = methodInfo.GetCustomAttributes(typeof(T), false);
            if (attributes != null)
                if (attributes.Length > 0)
                {
                    return true;
                }

            return false;
        }
        private static bool CheckAttribute<T>(Type type) where T : Attribute
        {
            object[] attributes = type.GetCustomAttributes(typeof(T), false);
            if (attributes != null)
                if (attributes.Length > 0)
                {
                    return true;
                }

            return false;
        }

        public static void StartInternalTimer()
        {
            QueryPerformanceCounter(out blaze_start);
            long freq;
            QueryPerformanceFrequency(out freq);

            clock_freq = freq / 1000d;
            Debug.Log($"Clock Frequency: {clock_freq}", System.Drawing.Color.OrangeRed, "Blazing");
        }

        public static void StartTimer()
        {
            QueryPerformanceCounter(out timer_start);
        }

        public static long StopTimer()
        {
            long now;
            QueryPerformanceCounter(out now);

            return (long)((now - timer_start) / clock_freq);
        }

        public static double StopTimerD()
        {
            long now;
            QueryPerformanceCounter(out now);

            return ((double)now - (double)timer_start) / clock_freq;
        }
    }
}
