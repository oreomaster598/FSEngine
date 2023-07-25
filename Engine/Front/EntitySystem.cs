using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine
{
	public class Entity
	{
		internal Region region;

		internal int x = 0, y = 0;
		public virtual void Save(BinaryWriter writer)
		{

		}
		public virtual void Load(BinaryReader reader)
		{

		}

		public void Move(int x, int y)
		{
			this.x += x;
			this.y += y;

			region.Check(this);
		}
		public virtual void Update()
		{
		}
	}


	public static class Entities
	{
		public static void Register(Entity entity)
		{
			Regions.AddEntity(entity);
		}
		public static void Unregister(Entity entity)
		{
			Regions.RemoveEntity(entity);
		}

		public static void Tick()
		{
			Regions.Update();
		}
	}
	internal static class Regions
	{
		public static Dictionary<(int, int), Region> regions = new Dictionary<(int, int), Region>();

		public static int x = 0, y = 0;
		public static int range = 1;


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
					regions.Remove((r.x, r.y));
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
		}
		public static void RemoveEntity(Entity entity)
		{
			entity.region.entities.Remove(entity);
		}
	}
	//chunk 60x60 cells
	//region 16x16 chunks  960x960 cells
	internal class Region
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
			if (!(entity.x / CellToRegion == x && entity.y / CellToRegion == y))
			{
				Regions.MoveToNewRegion(entity);
			}
		}

		public void Save(BinaryWriter writer)
		{
			for (int i = 0; i < entities.Count; i++)
			{
				entities[i].Save(writer);
			}
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
}
