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
    public static class State
    {
        #region Chunks


        //[Obsolete("Use SaveChunk(CellChunk chunk, Stream stream) instead.")]
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

        //[Obsolete("Use LoadChunk(Int32 cx, Int32 cy, Stream stream) instead.")]
        public static CellChunk LoadChunk(Int32 cx, Int32 cy, string dir)
        {
            CellChunk chunk = new CellChunk(cx, cy, true);
            chunk.has_file = true;
            using (FileStream stream = File.Open($"{dir}/chunk-{cx},{cy}.dat", FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    chunk.biome = reader.ReadInt16();

                    for (Int32 x = 0; x != CellWorld.chunk_s; x++)
                        for (Int32 y = 0; y != CellWorld.chunk_s; y++)
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

        public static void DeleteChunk(Int32 x, Int32 y, Stream stream, bool leaveOpen = false)
        {

        }

        public static void SaveChunk(CellChunk chunk, Stream stream, bool leaveOpen = false)
        {
            if (chunk.x < 0 || chunk.y < 0)
                return;
            List<Byte> bytes = new List<Byte>();

            bytes.AddRange(BitConverter.GetBytes(CellWorld.chunk_s));
            int i = 0;
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
            {
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
            if (i == 0)
            {
                DeleteChunk(chunk.x, chunk.y, stream, leaveOpen);
            }

        }
        public static CellChunk LoadChunk(Int32 cx, Int32 cy, Stream stream, bool leaveOpen = false)
        {

            CellChunk chunk = new CellChunk(cx, cy);
            chunk.has_file = true;
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen))
            {
                chunk.biome = reader.ReadInt16();

                for (Int32 x = 0; x != CellWorld.chunk_s; x++)
                    for (Int32 y = 0; y != CellWorld.chunk_s; y++)
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

        #endregion
    }
}
