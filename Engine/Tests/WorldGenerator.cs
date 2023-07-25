using FSEngine.CellSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.Tests
{
   

    public class CaveGenerator
    {
        public Noise noise;
        private double size;

        public CaveGenerator(int seed, double size, double frequency = 0.01, double persistence = 5)
        {
            int octaves = 2;
            double amplitude = 2;
            this.size = size;
            noise = new Noise(persistence, frequency, amplitude, octaves, seed);
        }
        public bool Generate(double x, double y)
        {
            return noise.GetWorm2D(x, y, 1, size) == 0;
        }
    }
    public class OreGenerator
    {
        public Noise noise;
        private double size;
        public short ore;

        public OreGenerator(short ore, int seed, double min, double max, double frequency = 0.05, double persistence = 0.5)
        {
            this.ore = ore;
            this.size = min;
            noise = new Noise(persistence, frequency, max, 2, seed);
        }
        public bool Generate(double x, double y)
        {
            return noise.Get2D(x, y) > size;
        }
    }

    public struct Biome
    {
        public int max_grass_height;
        public int min_grass_height;
        public int grass_spread;
        public int flower_chance;
        public int max_size;
        public int min_size;
        public float amplitude;
        public float frequency;
        public float Grass_R;
        public float Grass_G;
        public float Grass_B;
        public bool flowers;
        public bool trees;
        public bool grass_bed;
        public float brightness;

        public ushort sky;
        public short wood;
        public short leaf;
        public short dirt;
        public short rock;
        public short grass;
        public short id;

        public Biome(int maxX, int minX)
        {
            grass = (short)MaterialType.Grass;
            dirt = (short)MaterialType.Dirt;
            rock = (short)MaterialType.Rock;

            id = 0;
            flowers = true;
            trees = true;
            grass_bed = true;
            sky = 0;
            wood = 11;
            leaf = 17;
            Grass_R = 1;
            Grass_G = 1;
            Grass_B = 1;
            frequency = 0.05f;
            amplitude = 25;
            flower_chance = 12;
            min_size = minX;
            max_size = maxX;
            grass_spread = 4;
            min_grass_height = 1;
            max_grass_height = 3;
            brightness = 1;
        }
    }
    public static class Biomes
    {
        
        public static List<Biome> biomes = new List<Biome>();

        public static void Add(Biome b)
        {
            b.id = (short)biomes.Count;
            biomes.Add(b);
        }
        public static Biome Get(short id)
        {
            if (id > -1 && id < biomes.Count)
                return biomes[id];
            return new Biome();

        }

        public static Biome Random(ref Random rng)
        {
            return biomes[rng.Next(0, biomes.Count-1)] ;
        }
    }
    public struct Vector2I
    {
        public int x;
        public int y;

        public Vector2I(Int32 x, Int32 y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public enum WorldType
    {
        Regular,
        Void
    }

    public class WorldGenerator
    {
        public Random rng = new Random();
        public Noise terrain;
        public CaveGenerator cave;
        public OreGenerator[] oregens;
        public int seed = 75675;
        public int octaves = 2;
        public double persistence = 1;
        public float cavesize = 0.5f;
        public float cavepersistence = 5f;
        public int hill_origin = 25;
        public int world_offset = 8;
        public bool trees = true;
        public Biome biome =  new Biome(10, 5) { wood = (short)MaterialType.Wood, flowers = true, Grass_R = 1, Grass_G = 1, Grass_B = 1, frequency = 0.01f, amplitude = 50, flower_chance = 12, grass_spread = 2, max_grass_height = 3, min_grass_height = 1 };
        public Color[] flower_colors = new Color[] { Color.FromArgb(0x88, 0xBA, 0xCD), Color.FromArgb(0xFE, 0xB4, 0x60), Color.FromArgb(0xFE, 0x94, 0xC0), Color.FromArgb(0xE6, 0xED, 0xF7) };

        public static Structure pond;
        public static Structure geode;

        public List<Vector2> structure_locs = new List<Vector2>();
        public List<Structure> structures = new List<Structure>();


        public static int StringToSeed(string s)
        {
            int sum = 0;
            for (int i = 0; i < s.Length; i++)
            {
                sum += (s[i] * (i + 1));
            }
            return sum;
        }
        public int NextBiome = 0;
        
        public WorldGenerator()
        {
            seed = new Random().Next(0, int.MaxValue);
            Update();
            NextBiome = rng.Next(biome.min_size, biome.max_size);
            if(pond == null)
                pond = Structure.Load("Content/Structures/world_gen/pond.png", false);
            if (geode == null)
                geode = Structure.Load("Content/Structures/world_gen/geode.png", false);
        }
        public void SetOres(params OreGenerator[] generators)
        {
            for (int i = 0; i < generators.Length; i++)
            {
                generators[i].noise.randomSeed = seed + i;
            }
            oregens = generators;
        }
        public void Update()
        {
            SetOres(new OreGenerator((short)MaterialType.Moon_Rock, 0, 1.8, 4));
            cave = new CaveGenerator(seed, cavesize, 0.01, cavepersistence);
            terrain = new Noise(persistence, biome.frequency, biome.amplitude, octaves, seed);
        }
        static int surface = 600;
        public int GetHeight(Int32 x)
        {
            return surface + (int)terrain.Get1DInterpolated(x);
        }
        public CellChunk Gen(Int32 cx, Int32 cy)
        {
            CellChunk chunk = new CellChunk(cx, cy, false);
            chunk.biome = biome.id;

            for (int i = 0; i < CellWorld.chunk_s; i++)
            {
                int surface = GetHeight(cx * CellWorld.chunk_s + i);

                int y = cy * CellWorld.chunk_s;
                int rely = surface % CellWorld.chunk_s;
                if ((y < surface && y + CellWorld.chunk_s > surface) || y == surface)//&& y + CellWorld.chunk_s > surface
                {

                    chunk.cells[i, rely] = Sampler.GetSampler(biome.grass).GetCell(cx * CellWorld.chunk_s + i, surface + rely); chunk.filledcells++;
                    for (int z = rely + 1; z < CellWorld.chunk_s; z++)
                    {
                        chunk.cells[i, z] = Sampler.GetSampler(biome.dirt).GetCell(cx * CellWorld.chunk_s + i, surface + z);
                        chunk.filledcells++;
                    }
                }
                else if ((y < surface + CellWorld.chunk_s && y > surface) || y == surface + CellWorld.chunk_s)
                {
                    for (int z = rely; z > -1; z--)
                    {
                        chunk.cells[i, z] = Sampler.GetSampler(biome.dirt).GetCell(cx * CellWorld.chunk_s + i, surface + z);
                        chunk.filledcells++;
                    }

                    for (int z = rely; z < CellWorld.chunk_s; z++)
                    {
                        chunk.cells[i, z] = Sampler.GetSampler(biome.rock).GetCell(cx * CellWorld.chunk_s + i, surface + z);
                        chunk.filledcells++;

                    }
                }
                else if (y > surface + CellWorld.chunk_s)
                {
                    if (y > surface + 100)
                    {

                        chunk.biome = -1;
                        for (int z = 0; z < CellWorld.chunk_s; z++)
                        {
                            if (cave.Generate(cx * CellWorld.chunk_s + i, cy * CellWorld.chunk_s + z))
                            {
                                chunk.cells[i, z] = Sampler.GetSampler(biome.rock).GetCell(cx * CellWorld.chunk_s + i, surface + z);
                                chunk.filledcells++;
                                for (int ore = 0; ore < oregens.Length; ore++)
                                {
                                    if (oregens[ore].Generate(cx * CellWorld.chunk_s + i, cy * CellWorld.chunk_s + z))
                                    {
                                        chunk.cells[i, z] = Sampler.GetSampler(oregens[ore].ore).GetCell(cx * CellWorld.chunk_s + i, surface + z);
                                        chunk.filledcells++;
                                    }
                                }

                            }


                        }
                    }
                    else
                    {
                        for (int z = 0; z < CellWorld.chunk_s; z++)
                        {
                            chunk.cells[i, z] = Sampler.GetSampler(biome.rock).GetCell(cx * CellWorld.chunk_s + i, surface + z);
                            chunk.filledcells++;
                        }
                    }

                }
            }
            for (int i = 0; i < structure_locs.Count; i++)
            {

                if (structures[i].IsInChunk(chunk, (int)structure_locs[i].X, (int)structure_locs[i].Y))
                {
                    if(structures[i].CreateInChunk(chunk, (int)structure_locs[i].X, (int)structure_locs[i].Y))
                    {
                        structures.RemoveAt(i);
                        structure_locs.RemoveAt(i);
                        i--;
                    }
                }
            }
            return chunk;
        }
        public void SpawnStructs()
        {
            structures.Clear();
            structure_locs.Clear();

            for (int i = 0; i < 5; i++)
            {
                structure_locs.Add(new Vector2(rng.Next(0, 70) * CellWorld.chunk_s + rng.Next(0, CellWorld.chunk_s), rng.Next(12, 15) * CellWorld.chunk_s + rng.Next(0, CellWorld.chunk_s)));
                structures.Add(geode);
            }
        }
    }
}
