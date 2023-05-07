using FSEngine.IO;
using MemoryMgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.CellSystem
{

    [StructLayout(LayoutKind.Sequential, Size = 50, CharSet = CharSet.Ansi)]
    public struct Material
    {
        public byte Process;
        public Int16 Flammability;
        public UInt32 Density;
        public UInt32 Hardness;
        public Byte CollisionData;
        public Boolean IsStatic;
        public Int16 Life;  
        public Boolean CanMelt;
        public Int16 DispersionRate;
        public Int16 Corrosive;
        public Int16 CorrosionResist;
    }

    public enum MaterialType : short
    {
        Sand = 1,
        Iron,
        Water,
        Acid,
        Toxic_Gas,
        Lava,
        Glass,
        Ice,
        Liquid_Air,
        Suspicious_Liquid,
        Wood,
        Exotic_Matter,
        Rust,
        Gold,
        Mysterious_Stone,
        Rock,
        Grass,
        Dirt,
        Fire,
        Sandstone,
        Live_Wood,
        Moon_Rock,
        Salt,
        Brine,
        Ember,
        Silver,

    }
    public static unsafe class Materials
    {
        public static List<Material> materials = new List<Material>();
        public static Dictionary<short, int> items = new Dictionary<short, int>();
        private static short id = 1;

        public static Material Zero = new Material() { CollisionData = 0, Hardness = 9999, Density = 9999, Life = -1, Flammability = 0, IsStatic = true };


        public static void RegisterItem(short type, int itemid)
        {
            items.Add(type, itemid);
        }
        public static void Add(Material mat)
        {
            if (Sampler.mappings.Count < materials.Count)
                throw new Exception("Materials must have a sampler associated with them.");
            materials.Add(mat);
            id++;
        }
        public static void Add(Material mat, string sampler)
        {
            materials.Add(mat);
            Sampler.Map(new Sampler(sampler), id);
            id++;
        }
        public static Material Get(Int16 type)
        {

            if (type < 1)
                return Zero;
            return materials[type - 1];
        }
        public static void DeleteAll()
        {
            Sampler.DeleteSamplers();
            id = 1;
            materials.Clear();
        }
        /*public static void LoadFromHeap()
        {
            State.SetDirectory("Content/Materials");

            Manifest manifest = State.LoadManifest<Material>();
            Material[] materials;
            using (Heap heap = Heap.FromBytes(File.ReadAllBytes("Content/Materials/Materials.heap"), HeapFormat.Stride16))
            {
                materials = heap.GetAll<Material>(1, heap.Get<Int32>(0));
            }

            foreach (CellData data in manifest.data)
            {
                Language.AddLocalization($"material.mat_{id}.name", data.localization);
                Add(materials[data.materialtype], data.texture);
            }
        }
        public static void Load(Manifest manifest, Material[] mats)
        {
            foreach (CellData data in manifest.data)
            {
                Language.AddLocalization($"material.mat_{id}.name", data.localization);
                Add(mats[data.materialtype], data.texture);
            }
        }*/
    }
}
