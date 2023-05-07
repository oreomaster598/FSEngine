using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FSEngine.CellSystem;
using FSEngine.Tiles;
using ImGuiNET;
using MemoryMgr;

namespace FSEngine.IO
{
    public enum CellReadMethod : byte
    {
        NULL = 0b00000001,
        DEFAULT = 0b00000010,
        EXTRADATA = 0b00000100,
    }
    public struct CellData
    {
        public string texture;
        public string localization;
        public int materialtype;

        public CellData(string name, string text, int type)
        {
            texture = text;
            localization = name;
            materialtype = type;
        }
    }
    public class Manifest
    {
        internal List<CellData> data = new List<CellData>();
    }
    public static class State
    {
        public static string sdir = "";
        public static void SetDirectoryS(string dir) => sdir = Path.Combine(Directory.GetCurrentDirectory(), "Saves", dir);
        public static void SetDirectory(string dir) => sdir = Path.Combine(Directory.GetCurrentDirectory(), dir);
        #region Chunks
        public static void SaveChunk(CellChunk chunk, string dir)
        {
            if (chunk.x < 0 || chunk.y < 0)
                return;
            List<Byte> bytes = new List<Byte>();

            bytes.AddRange(BitConverter.GetBytes(CellWorld.chunk_s));
            int i = 0;
            using (FileStream stream = File.Open($"{dir}/chunk-{chunk.x},{chunk.y}.dat", FileMode.OpenOrCreate))
            {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    writer.Write(CellWorld.chunk_s);
                    writer.Write(chunk.biome);
                    
                    for (int x = 0; x != CellWorld.chunk_s; x++)
                        for (int y = 0; y != CellWorld.chunk_s; y++)
                        {
                            Cell cell = chunk.GetCell(x, y);
                            CellReadMethod readMethod = CellReadMethod.DEFAULT;
                            if (cell.type == 0 || cell.owner > 0) readMethod = CellReadMethod.NULL;

                            else if (cell.UserData > 0) readMethod |= CellReadMethod.EXTRADATA;

                            writer.Write((byte)readMethod);
                            if ((readMethod & CellReadMethod.NULL) == 0)
                            {
                                i++;
                                writer.Write(Color.FromArgb(cell.A, cell.R, cell.G, cell.B).ToArgb());
                                writer.Write(cell.type);
                                writer.Write(cell.owner);
                                writer.Write(cell.life);
                                writer.Write(cell.paint);
                                if ((readMethod & CellReadMethod.EXTRADATA) != 0)
                                {
                                    writer.Write(cell.UserData);
                                }
                            }
                        }
                }

            }
            if (i == 0)
            {
                File.Delete($"{dir}/chunk-{chunk.x},{chunk.y}.dat");
            }

        }
       
        public static CellChunk LoadChunk(Int32 cx, Int32 cy, string dir)
        {
            UInt16 chunk_s;

            CellChunk chunk = new CellChunk(cx, cy);
            chunk.has_file = true;
            using (FileStream stream = File.Open($"{dir}/chunk-{cx},{cy}.dat", FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    chunk_s = reader.ReadUInt16();
                    chunk.biome = reader.ReadInt16();

                    for (Int32 x = 0; x != chunk_s; x++)
                        for (Int32 y = 0; y != chunk_s; y++)
                        {
                            CellReadMethod readMethod = (CellReadMethod)reader.ReadByte();
                            if ((readMethod & CellReadMethod.NULL) == 0)
                            {
                                Cell cell = new Cell();

                                if ((readMethod & CellReadMethod.DEFAULT) != 0)
                                {
                                    Color cl = Color.FromArgb(reader.ReadInt32());
                                    cell.A = cl.A;
                                    cell.R = cl.R;
                                    cell.G = cl.G;
                                    cell.B = cl.B;
                                    cell.type = reader.ReadInt16();
                                    cell.owner = reader.ReadInt16();
                                    cell.life = reader.ReadInt16();
                                    cell.paint = reader.ReadByte();
                                    //chunk.filledcells++;
                                }
                                if ((readMethod & CellReadMethod.EXTRADATA) != 0)
                                    cell.UserData = reader.ReadInt16();
                                chunk.cells[x, y] = cell;
                                if (cell.type > 0)
                                    chunk.filledcells++;
                            }
                        }
                }
            }
            return chunk;
        }
        public static Bitmap ChunkToBitmap(Int32 x, Int32 y, string dir)
        {
            CellChunk c = LoadChunk(x, y, dir);
            Bitmap bmp = new Bitmap(CellWorld.chunk_s, CellWorld.chunk_s);
            for (Int32 cy = 0; cy != CellWorld.chunk_s; cy++)
                for (Int32 cx = 0; cx != CellWorld.chunk_s; cx++)
                {
                    Cell ce = c.GetCell(cx, cy);
                    bmp.SetPixel(cx, cy, Color.FromArgb(ce.A, ce.R, ce.G, ce.B));
                }
            return bmp;
        }
        /*public static SKBitmap ChunkToBitmap(CellChunk c)
        {
            Bitmap bmp = new Bitmap(CellWorld.chunk_s, CellWorld.chunk_s);
            for (Int32 cy = 0; cy != CellWorld.chunk_s; cy++)
                for (Int32 cx = 0; cx != CellWorld.chunk_s; cx++)
                {
                    Cell ce = c.GetCell(cx, cy);
                   bmp.SetPixel(cx, cy,  Color.FromArgb(ce.A, ce.R, ce.G, ce.B));
                }
            return bmp.ToSKBitmap();
        }*/
        public static Bitmap ChunkToBitmap(Int32 x, Int32 y, CellWorld world)
        {
            CellChunk c = new CellChunk(0, 0);//= world.chunks[new Vector2(x, y)];
            Bitmap bmp = new Bitmap(CellWorld.chunk_s, CellWorld.chunk_s);
            for (Int32 cy = 0; cy != CellWorld.chunk_s; cy++)
                for (Int32 cx = 0; cx != CellWorld.chunk_s; cx++)
                {
                    Cell ce = c.GetCell(cx, cy);
                    bmp.SetPixel(cx, cy, Color.FromArgb(ce.A, ce.R, ce.G, ce.B));
                }
            return bmp;
        }
        #endregion
        #region Structs
        public static int[] GetSessions()
        {
            List<int> sessions = new List<int>();

            if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Saves")))
                foreach (string s in Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "Saves"), "save*"))
                {
                    int id = int.Parse(s.Substring(s.LastIndexOfAny("\\/".ToCharArray())).Replace("\\", string.Empty).Replace("/", string.Empty).Remove(0, 4));
                    sessions.Add(id);
                }

            return sessions.ToArray();
        }

        public static Manifest LoadManifest<T>()
        {
            Manifest manifest = new Manifest();
            using (FileStream stream = File.Open($"{sdir}/{typeof(T).Name}.manifest", FileMode.OpenOrCreate))
            {
                manifest = LoadManifest<T>(stream);

            }
            return manifest;
        }
        public static void SaveManifest<T>(Manifest manifest)
        {
            using (FileStream stream = File.Open($"{sdir}/{typeof(T).Name}.manifest", FileMode.OpenOrCreate))
            {
                SaveManifest<T>(manifest, stream);

            }
        }
        public static Manifest LoadManifest<T>(Stream stream, bool LeaveOpen = false)
        {
            Manifest manifest = new Manifest();

            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, LeaveOpen))
            {
                int size = reader.ReadInt32();
                for (int i = 0; i != size; i++)
                {
                    manifest.data.Add(new CellData(reader.ReadString(), reader.ReadString(), reader.ReadInt32()));
                }
            }

            return manifest;
        }
        public static void SaveManifest<T>(Manifest manifest, Stream stream, bool LeaveOpen = false)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, LeaveOpen))
            {
                writer.Write(manifest.data.Count);
                foreach (CellData data in manifest.data)
                {
                    writer.Write(data.localization);
                    writer.Write(data.texture);
                    writer.Write(data.materialtype);
                }
            }
        }
        static int stucid = 0;
        public static unsafe void SaveStruct<T>(T struc) where T : unmanaged
        {
            byte[] bytes = Memory.ToBytes(&struc);
            string name = typeof(T).Name;

            string ext = name.ToLower();
            if (ext.Length > 3) ext = ext.Remove(3);
            File.WriteAllBytes($"{sdir}/{name}_{stucid}.{ext}", bytes);
            stucid++;
        }
        public static unsafe T LoadStruct<T>(int id) where T : unmanaged
        {
            string name = typeof(T).Name;
            string ext = name.ToLower();
            if (ext.Length > 3) ext = ext.Remove(3);
            byte[] bytes = File.ReadAllBytes($"{sdir}/{name}_{id}.{ext}");
            return Memory.ToStruct<T>(bytes);
        }
        #endregion
    }
}
