using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using System.IO;
using System.Runtime.InteropServices;
using FSEngine.IO;
using MemoryMgr;

namespace FSEngine.CellSystem
{
    public struct Cell
    {
        public static Cell Zero = new Cell(0);
        public UInt32 frame;

        public Int16 type;


        public Int16 UserData;
        public Int16 life;

        /// <summary>
        /// Owner ID.
        /// If zero its owned by the world.
        /// </summary>
        public Int16 owner;
        public Byte paint;


        public Byte A, R, G, B;

        public Int16 heat;
        //public Color c = Color.Transparent;
        public Cell(short type = 0)
        {
            this.life = -1;
            frame = 0;
            this.type = type;
            UserData = 0;
            owner = 0;
            paint = 0;
            heat = 0;
            A = 0;
            R = 0;
            G = 0;
            B = 0;
        }

        public static Color GetEffect(Cell cell)
        {
            //Effect e = new Effect();
            //if (Materials.effects.ContainsKey(cell.type))
            //   e = Materials.effects[cell.type];
            return Color.FromArgb(0, 0, cell.type, 0);
        }

        //public override string ToString() => $"Color: [{R},{G},{B}] | Type: {type} | Life: {life} | Temperature: {heat}";
    }
}
