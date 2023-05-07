using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace MemoryMgr
{
    public enum StorageMedium : int
    {
        Byte = 1,
        KB = 1000,
        MB = 1000000,
        GB = 1000000000,
        KIB = 1024,
        MIB = 1048576,
        GIB = 1073741824,
    }
    public enum HeapFormat
    {
        Unformatted = 0,
        Stride32 = 4,
        Stride16 = 2,
    }

    public unsafe class UnmanagedStreamReader
    {
        private int pointer = 0;
        private UnmanagedMemory* block;

        public UnmanagedStreamReader(UnmanagedMemory* block)
        {
            this.block = block;
        }
        public T Read<T>() where T : unmanaged
        {
            T result = Memory.Get<T>(*block, pointer);
            
            pointer += Memory.SizeOf<T>();
            return result;
        }
        public Int64 ReadInt64()
        {
            byte[] buffer = new byte[8];

            Memory.Copy(*block, pointer, ref buffer, 8);

            pointer += 8;
            return BitConverter.ToInt64(buffer, 0);
        }
        public Int32 ReadInt32()
        {
            byte[] buffer = new byte[4];
            
            Memory.Copy(*block, pointer, ref buffer, 4);
            
            pointer += 4;
            return BitConverter.ToInt32(buffer, 0);
        }
        public Int16 ReadInt16()
        {
            byte[] buffer = new byte[2];

            Memory.Copy(*block, pointer, ref buffer, 2);

            pointer += 2;
            return BitConverter.ToInt16(buffer, 0); ;
        }
        public Byte ReadInt8()
        {
            pointer += 1;
            return *((*block)[pointer]);
        }
    }
    public unsafe class UnmanagedStreamWriter
    {
        private int pointer = 0;
        private UnmanagedMemory* block;

        public UnmanagedStreamWriter(UnmanagedMemory* block)
        {
            this.block = block;
        }
        public void Write<T>(T obj) where T : unmanaged
        {
            int size = Memory.SizeOf<T>();
            Byte[] buffer = Memory.ToBytes(&obj);

            Memory.Copy(buffer, *block, pointer, size);

            pointer += size;
        }
    }
    public static unsafe class Memory
    {
        public static int Convert(StorageMedium mediumIn, StorageMedium mediumOut, int value) => value * (int)mediumIn / (int)mediumOut;
        public static void Copy(IntPtr src, IntPtr dest, int size, StorageMedium medium = StorageMedium.Byte)
        {
            size = Memory.Convert(medium, StorageMedium.Byte, size);

            Byte[] bytes = new Byte[size];

            Marshal.Copy(src, bytes, 0, size);
            Marshal.Copy(bytes, 0, dest, size);
        }
        
        public static void Copy(UnmanagedMemory memory, int startindex, ref byte[] dest, int size)
        {
            Marshal.Copy((IntPtr)memory[startindex], dest, 0, size);
        }
        public static void Copy(byte[] src, UnmanagedMemory memory, int startindex, int size)
        {
            Marshal.Copy(src, 0, (IntPtr)memory[startindex], size);
        }
        public static T Get<T>(UnmanagedMemory memory, int startindex) where T : unmanaged
        {
            int size = Marshal.SizeOf(typeof(T));

            T obj = new T();
            Byte[] data = new Byte[size];

            Marshal.Copy((IntPtr)(memory[startindex]), data, 0, size);
            Marshal.Copy(data, 0, (IntPtr)(&obj), size);

            return obj;
        }
        public static unsafe byte[] ToBytes<T>(T* obj) where T : unmanaged
        {
            byte[] bytes;

            int size = Marshal.SizeOf(typeof(T));

            bytes = new byte[size];

            Marshal.Copy((IntPtr)obj, bytes, 0, size);

            return bytes;
        }
        public static unsafe T ToStruct<T>(byte[] bytes) where T : unmanaged
        {
            int size = Marshal.SizeOf(typeof(T));

            T str = new T();

            if (bytes.Length == size)
                Marshal.Copy(bytes, 0, (IntPtr)(&str), size);

            return str;
        }

        public static Int32 SizeOf<T>() where T : unmanaged
        {
            return Marshal.SizeOf(typeof(T));
        }

    }
    /// <summary>
    /// A group of methods to access unmanaged memory.
    /// </summary>
    public unsafe class Heap : IDisposable
    {
        public UnmanagedMemory memory;
        public int stack_ptr;
        private HeapFormat format;

        public static Heap Alloc(int bytes, HeapFormat format = HeapFormat.Unformatted)
        {
            Heap heap = new Heap();

            heap.memory = UnmanagedMemory.Alloc(bytes);
            heap.stack_ptr = 0;
            heap.format = format;

            return heap;
        }
        public static Heap Alloc(int size, StorageMedium medium, HeapFormat format = HeapFormat.Unformatted)
        {
            Heap heap = new Heap();

            heap.memory = UnmanagedMemory.Alloc(Memory.Convert(medium, StorageMedium.Byte, size));
            heap.stack_ptr = 0;
            heap.format = format;

            return heap;
        }
        public static Heap FromBytes(Byte[] bytes, HeapFormat format = HeapFormat.Unformatted)
        {
            Heap heap = new Heap();

            heap.memory = UnmanagedMemory.Alloc(bytes.Length);
            heap.stack_ptr = 0;
            heap.format = format;

            Marshal.Copy(bytes, 0, heap.memory, bytes.Length);

            return heap;
        }
        public T[] GetAll<T>(int startindex, int count) where T : unmanaged
        {
            List<T> types = new List<T>();
            for (int i = startindex; i != startindex + count; i++)
            {
                types.Add(Get<T>(GetIndex(i)));
            }
            return types.ToArray();
        }
        /// <summary>
        /// Only call if the heap is from an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetAll<T>() where T : unmanaged
        {
            int stride = Marshal.SizeOf(typeof(T)) + (int)format;
            List<T> types = new List<T>();
            for (int i = 0; i != (memory.length / stride); i++)
            {
                types.Add(Get<T>(GetIndex(i)));
            }
            return types.ToArray();
        }
        public static Heap FromArray<T>(T[] array, HeapFormat format = HeapFormat.Unformatted) where T : unmanaged
        {
            int size = Marshal.SizeOf(typeof(T)) + (int)format;

            Heap heap = Alloc((size * array.Length) + 4 + (int)format, format);

            heap.Add(array.Length);

            for (int i = 0; i != array.Length; i++)
            {
                heap.Add(array[i]);
            }


            return heap;
        }
        public void Add<T>(T obj) where T : unmanaged
        {
            int size = Marshal.SizeOf(typeof(T));

            Byte[] bytes = new Byte[size];

            SetStride(stack_ptr, size);

            Marshal.Copy((IntPtr)(&obj), bytes, 0, size);
            MoveOntoHeap(stack_ptr + (int)format, bytes);

            stack_ptr += (int)format + size;
        }
        public T Get<T>(int index) where T : unmanaged
        {
            int size = Marshal.SizeOf(typeof(T));

            T obj = new T();
            Byte[] data = new Byte[size];

            Marshal.Copy((IntPtr)(memory[index + (int)format]), data, 0, size);
            Marshal.Copy(data, 0, (IntPtr)(&obj), size);

            return obj;
        }
        public int GetIndex(int position)
        {
            if (format == HeapFormat.Unformatted)
                return -1;

            int index = 0;
            int i = 0;
            while (index != position)
            {
                int stride = GetStride(i);
                i += stride + (int)format;
                index++;
            }
            return i;
        }
        public void SetStride(int index, int stride)
        {
            if (format == HeapFormat.Stride32)
            {
                MoveOntoHeap(index, BitConverter.GetBytes(stride));
                return;
            }
            if (format == HeapFormat.Stride16)
            {
                MoveOntoHeap(index, BitConverter.GetBytes((short)stride));
                return;
            }
        }
        public int GetStride(int index)
        {
            if (format == HeapFormat.Stride32)
            {
                return BitConverter.ToInt32(new byte[] { *memory[index], *memory[index + 1], *memory[index + 2], *memory[index + 3] }, 0);
            }
            if (format == HeapFormat.Stride16)
            {
                return BitConverter.ToInt16(new byte[] { *memory[index], *memory[index + 1] }, 0);
            }
            return -1;
        }
        public void MoveOntoHeap(int index, Byte[] bytes) => Marshal.Copy(bytes, 0, (IntPtr)memory[index], bytes.Length);
        public void Dump(string path, bool free)
        {
            Byte[] bytes = new Byte[memory.length];
            Marshal.Copy((IntPtr)memory.handle, bytes, 0, memory.length);
            if (free) memory.Free();
            File.WriteAllBytes(path, bytes);
        }
        public void Dump(Stream stream, bool free)
        {
            Byte[] bytes = new Byte[memory.length];
            Marshal.Copy((IntPtr)memory.handle, bytes, 0, memory.length);
            if (free) memory.Free();

            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }

        }
        public static Heap FromStream(Stream stream, HeapFormat format)
        {
            Heap heap;
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
            {
                int len = reader.ReadInt32();
                byte[] buf = reader.ReadBytes(len);
                heap = FromBytes(buf, format);
            }
            return heap;
        }
        public void Dispose()
        {
            memory.Free();
        }
    }
    /// <summary>
    /// Provides raw access to unmanaged memory.
    /// </summary>
    public unsafe struct UnmanagedMemory
    {
        public Byte* handle;
        public int length;

        public static UnmanagedMemory Alloc(int length)
        {
            UnmanagedMemory memory = new UnmanagedMemory();
            memory.length = length;
            memory.handle = (Byte*)Marshal.AllocHGlobal(length);
            return memory;
        }

        public void Free()
        {
            Marshal.FreeHGlobal((IntPtr)handle);
        }

        public Byte* this[int index]
        {
            get
            {
                return &handle[index];
            }
        }
        public static implicit operator Byte*(UnmanagedMemory mem) => mem.handle;
        public static implicit operator IntPtr(UnmanagedMemory mem) => (IntPtr)mem.handle;
    }
}
