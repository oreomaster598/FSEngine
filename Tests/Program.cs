using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tests 
{

    public class WeightedRandom<T>
    {
        class WeightedItem<T>
        {
            public T value;
            public double weight;

            public WeightedItem(T value, double weight)
            {
                this.value = value;
                this.weight = weight;
            }
        }

        List<WeightedItem<T>> items = new List<WeightedItem<T>>();

        double total_weight;
        uint seed = 1;
        public int length = 0;
        
        public WeightedRandom(uint seed)
        {
            this.seed = seed;
        }
        public WeightedRandom()
        {
            this.seed = (uint)DateTime.Now.Millisecond;
        }
        public void Add(T item, double weight)
        {
            total_weight += weight;

            items.Add(new WeightedItem<T>(item, weight));

            length++;
        }
        public void Remove(T item)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].value.Equals(item))
                {
                    total_weight -= items[i].weight;
                    items.RemoveAt(i);
                    length--;
                    return;
                }
            }

        }
        double Nextdouble(double min, double max)
        {
            unchecked
            {
                seed = (16807 * seed) % 2147483647;
            }
            return min + (seed / 2147483647d * (max - min));
        }
        public T Pick()
        {
            double randomWeight = Nextdouble(0, total_weight / items.Count);
            for (int i = 0; i < items.Count; ++i)
            {
                randomWeight -= (items[i].weight / items.Count);
                if (randomWeight < 0)
                {
                    return items[i].value;
                }
            }
            return default(T);
        }
    }
    public class Entity
    {
        public Region region;


        int i = 0;
        public int x = 1000, y = 1000;
        public void Save(BinaryWriter writer)
        {

        }
        public void Load(BinaryReader reader)
        {

        }

        public void Move(int x, int y)
        {
            this.x += x;
            this.y += y;

            region.Check(this);
        }
        public void Update()
        {
            Move(200, 200);
            i++;
            Console.WriteLine("Tick {0}",i);
        }
    }

    public static class Regions
    {
        public static Dictionary<(int, int), Region> regions = new Dictionary<(int, int), Region>();

        public static int x = 0, y = 0;
        public static int range = 2;


        public static void Update()
        {
            for (int i = 0; i < regions.Count; i++)
			{
				Region r = regions.ElementAt(i).Value;
				if (Math.Abs(r.x - x) != range || Math.Abs(r.y - y) != range)
                {
					r.Update();
                }

            }
            //x -= 1;
            for (int i = 0; i < regions.Count; i++)
            {
                Region r = regions.ElementAt(i).Value;
                if (Math.Abs(r.x - x) > range || Math.Abs(r.y - y) > range)
                {
                    r.Save(null);
                    regions.Remove((r.x,r.y));
                    i--;
                }
       
            }
        }

        public static void MoveToNewRegion(Entity entity)
        {
            (int, int) key = (entity.x / Region.CellToRegion, entity.y / Region.CellToRegion);
            if (regions.TryGetValue(key, out Region region))
            {
                entity.region.entities.Remove(entity);
                region.entities.Add(entity);
                entity.region = region;
            }
            else
            {
                Region region1 = new Region(key.Item1, key.Item2);
                regions.Add(key, region1);

                entity.region.entities.Remove(entity);
                region1.entities.Add(entity);
                entity.region = region1;
            }
            Console.WriteLine("Entity relocated");
        }

        public static void AddEntity(Entity entity)
        {
			(int, int) key = (entity.x / Region.CellToRegion, entity.y / Region.CellToRegion);
			if (regions.TryGetValue(key, out Region region))
			{
				region.entities.Add(entity);
				entity.region = region;
			}
			else
			{
				Region region1 = new Region(key.Item1, key.Item2);
				regions.Add(key, region1);

				region1.entities.Add(entity);
				entity.region = region1;
			}
			Console.WriteLine("Entity added");
		}
    }
    //chunk 60x60 cells
    //region 16x16 chunks  960x960 cells
    public class Region
    {
        public List<Entity> entities = new List<Entity>();
        public int x, y;
        public const int CellToRegion = 960;


        public Region(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public void Check(Entity entity)
        {
            if(!(entity.x / CellToRegion == x && entity.y / CellToRegion == y))
            {
                Regions.MoveToNewRegion(entity);
            }
        }

        public void Save(BinaryWriter writer)
        {
            Console.WriteLine("Region {0},{1} Saved",x,y);
        }
        public void Load(BinaryReader reader)
        {

        }
        public void Update()
        {
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].Update();
            }
        }
   }

    internal class Program
    {
        unsafe static void Main(string[] args)
        {
            Regions.AddEntity(new Entity());
            for (int i = 0; i < 20; i++)
            {
                Regions.Update();
            }
            Console.ReadLine();
        }
    }
}
