using FSEngine.CellSystem;
using FSEngine.Front.CellSystem.Events;
using FSEngine.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.Tests
{
	public enum Processor : byte
	{
		None = 0,
		Gas,
		Sand,
		Liquid,
		Salt,
		Fire,
		Acid,
		Ember
	}
	public class TestRasterizer : CellRasterizer
    {
        public override Color RasterizeCell(Cell cell, int x, int y, uint sx, uint sy)
        {
			Color c;
			if (cell.type == (short)MaterialType.Fire)
			{
				c = Color.FromHSL(25, 1, (1 - cell.life / (68f - FastRand((uint)x, (uint)y, 15))) * 0.85f);
				CellWorld.lights.SetPixel(sx, sy, Color.FromArgb(255, c.R, c.G, c.B));
				return c;
			}
			if (cell.type == (short)MaterialType.Ember)
			{
				c = Color.FromHSL(25, 1, (1 - cell.life / 80f) * 0.85f);
				CellWorld.lights.SetPixel(sx, sy, Color.FromArgb(255, c.R, c.G, c.B));
				return c;
			}
			if (cell.type == (short)MaterialType.Lava || cell.type == (short)MaterialType.Acid || cell.type == (short)MaterialType.Toxic_Gas)
			{
				CellWorld.lights.SetPixel(sx, sy, Color.FromArgb(255, cell.R, cell.G, cell.B));
				return Color.FromArgb(cell.A, cell.R, cell.G, cell.B);
			}
			return Color.FromArgb(cell.A, cell.R, cell.G, cell.B);
        }
    }
	public class TestWorker : ChunkWorker
	{
		public TestWorker(CellChunk chunk, CellWorld world) : base(chunk, world)
		{
		}

		public override bool UpdateCell(Cell c, int x, int y, ref int _x, ref int _y, double rn)
		{
			bool cease = false;

			Boolean swapd = false;
			Boolean swapu = false;
			if (world.InBounds(_x, _y + 1) && !world.IsEmpty(_x, _y + 1))
				swapd = Materials.Get(world.GetCell(_x, _y + 1).type).Density < material.Density;
			if (world.InBounds(_x, _y + 1) && !world.IsEmpty(_x, _y - 1))
				swapu = Materials.Get(world.GetCell(_x, _y - 1).type).Density < material.Density;
			bool d_left, d_right, left, right, up, down;
			switch (material.Process)
			{
				case (byte)Processor.Sand:
					{



							left = world.IsEmpty(_x - 1, _y + 1);
							right = world.IsEmpty(_x + 1, _y + 1);
							down = world.IsEmpty(_x, _y + 1);
							if (down)
							{
								_y++;
								//EventManager.QueueEvent(Game.SandFallEvent);
							}
							else if (swapd)
							{
								Swap(_x, _y, _x, _y + 1);

							}

							else if (left && right)
							{
								if ((int)(2 * rn) == 0)
									_x--;
								else
									_x++;
								_y++;
							}
							else if (left)
							{
								_x--;
								_y++;
							}
							else if (right)
							{
								_x++;
								_y++;
							}
	
						break;
					}
				case (byte)Processor.Liquid:
					{
						d_left = world.IsEmpty(_x - 1, _y + 1);
						d_right = world.IsEmpty(_x + 1, _y + 1);
						left = world.IsEmpty(_x - 1, _y);
						right = world.IsEmpty(_x + 1, _y);
						down = world.IsEmpty(_x, _y + 1);
						if (swapd)
						{
							Swap(_x, _y, _x, _y + 1);

						}
						else if (down)
						{
							_y++;
						}
						else if (d_left && d_right)
						{
							if (world.rng.Next(0, 2) == 0)
								_x--;
							else
								_x++;
							_y++;

						}
						else if (left && right)
						{
							if (world.rng.Next(0, 2) == 0)
								_x--;
							else
								_x++;

						}
						else if (d_left)
						{
							_x--;
							_y++;

						}
						else if (left)
						{
							_x--;

						}
						else if (d_right)
						{
							_x++;
							_y++;

						}
						else if (right)
						{
							_x++;

						}



						break;
					}
				case (byte)Processor.Gas:
					{
						d_left = world.IsEmpty(_x - 1, _y - 1);
						d_right = world.IsEmpty(_x + 1, _y - 1);
						left = world.IsEmpty(_x - 1, _y);
						right = world.IsEmpty(_x + 1, _y);
						down = world.IsEmpty(_x, _y - 1);
						if (swapu)
						{
							Swap(_x, _y, _x, _y - 1);

						}
						else if (d_left && d_right)
						{
							if (world.rng.Next(0, 2) == 0)
								_x--;
							else
								_x++;
							_y--;

						}
						else if (d_left)
						{
							_x--;
							_y--;
						}
						else if (d_right)
						{
							_x++;
							_y--;
						}
						else if (down)
						{
							_y--;
						}
						else if (left && right)
						{
							if (world.rng.Next(0, 2) == 0)
								_x--;
							else
								_x++;
						}
						else if (left)
						{
							_x--;
						}
						else if (right)
						{
							_x++;
						}
						break;
					}
				case (byte)Processor.Salt:
					{
						if (IsTouching(_x, _y, (short)MaterialType.Water))
						{
							world.ChangeCell(_x, _y, (short)MaterialType.Brine);
							cease = true;
							break;
						}
						left = world.IsEmpty(_x - 1, _y + 1);
						right = world.IsEmpty(_x + 1, _y + 1);
						down = world.IsEmpty(_x, _y + 1);
						if (swapd)
						{
							Swap(_x, _y, _x, _y + 1);

						}
						else if (left && right)
						{
							if (world.rng.Next(0, 2) == 0)
								_x--;
							else
								_x++;
							_y++;

						}
						else if (left)
						{
							_x--;
							_y++;

						}
						else if (right)
						{
							_x++;
							_y++;

						}
						else if (down)
						{
							_y++;

						}
						break;
					}
				case (byte)Processor.Fire:
					{
						c.life++;
						if (c.life < 60)
							chunk.SetCell(x, y, c);
						else
						{
							chunk.Clear(x, y);
							if (world.IsEmpty(_x, _y - 1) && world.frame % (int)(60 * rn + 1) == 0)
							{
								world.ChangeCell(_x, _y - 1, (short)MaterialType.Ember);
							}
						}

						if (world.frame % (int)(20 * rn + 1) == 0)
						{
							if (!world.IsEmpty(_x + 1, _y))
							{
								if (Materials.Get(world.GetCell(_x + 1, _y).type).Flammability > 0)
								{
									world.ChangeCell(_x + 1, _y, c.type);
								}
							}
							if (!world.IsEmpty(_x - 1, _y))
							{
								if (Materials.Get(world.GetCell(_x - 1, _y).type).Flammability > 0)
								{
									world.ChangeCell(_x - 1, _y, c.type);
								}
							}
							if (!world.IsEmpty(_x, _y + 1))
							{
								if (Materials.Get(world.GetCell(_x, _y + 1).type).Flammability > 0)
								{
									world.ChangeCell(_x, _y + 1, c.type);
								}
							}
							if (!world.IsEmpty(_x, _y - 1))
							{
								if (Materials.Get(world.GetCell(_x, _y - 1).type).Flammability > 0)
								{
									world.ChangeCell(_x, _y - 1, c.type);
								}
							}
						}

						cease = true;
						break;
					}
				case (byte)Processor.Acid:
					{
						if (world.frame % 10 == 0)
							Corrode(_x, _y);
						d_left = world.IsEmpty(_x - 1, _y + 1);
						d_right = world.IsEmpty(_x + 1, _y + 1);
						left = world.IsEmpty(_x - 1, _y);
						right = world.IsEmpty(_x + 1, _y);
						down = world.IsEmpty(_x, _y + 1);
						if (swapd)
						{
							Swap(_x, _y, _x, _y + 1);

						}
						else if (d_left && d_right)
						{
							if (world.rng.Next(0, 2) == 0)
								_x--;
							else
								_x++;
							_y++;

						}
						else if (d_left)
						{
							_x--;
							_y++;

						}
						else if (d_right)
						{
							_x++;
							_y++;

						}
						else if (down)
						{
							_y++;
						}
						else if (left && right)
						{
							if (world.rng.Next(0, 2) == 0)
								_x--;
							else
								_x++;

						}
						else if (left)
						{
							_x--;

						}
						else if (right)
						{
							_x++;

						}
						break;
					}
				case (byte)Processor.Ember:
					{
						c.life++;
						if (c.life < 80)
							chunk.SetCell(x, y, c);
						else
							chunk.Clear(x, y);

;
						if ((int)(3 * rn) == 0)
						{

							left = world.IsEmpty(_x - 1, _y);
							right = world.IsEmpty(_x + 1, _y);
							if (left && right)
							{
								if (world.rng.Next(0, 2) == 0)
									_x--;
								else
									_x++;

							}
							else if (left)
							{
								_x--;

							}
							else if (right)
							{
								_x++;

							}
						}
						else
                        {
							up = world.IsEmpty(_x, _y - 1);
							if (up)
							{
								_y--;
							}
						}


						if (world.frame % (int)(120 * rn + 1) == 0)
						{
							int flam;
							if (!world.IsEmpty(_x + 1, _y))
							{
								flam = Materials.Get(world.GetCell(_x + 1, _y).type).Flammability;
								if (flam > 0 && flam <= 100)
								{
									world.ChangeCell(_x + 1, _y, (short)(MaterialType.Fire));
								}
							}
							if (!world.IsEmpty(_x - 1, _y))
							{
								flam = Materials.Get(world.GetCell(_x - 1, _y).type).Flammability;
								if (flam > 0 && flam <= 100)
								{
									world.ChangeCell(_x - 1, _y, (short)(MaterialType.Fire));
								}
							}
							if (!world.IsEmpty(_x, _y + 1))
							{
								flam = Materials.Get(world.GetCell(_x, _y + 1).type).Flammability;
								if (flam > 0 && flam <= 100)
								{
									world.ChangeCell(_x, _y + 1, (short)(MaterialType.Fire));
								}
							}
							if (!world.IsEmpty(_x, _y - 1))
							{
								flam = Materials.Get(world.GetCell(_x, _y - 1).type).Flammability;
								if (flam > 0 && flam <= 100)
								{
									world.ChangeCell(_x, _y - 1, (short)(MaterialType.Fire));
								}
							}
						}
						break;
					}
			}
			return cease;
		}
	}
	public class ChunkManager : IChunkManager
	{
		public (CellChunk, bool) Generate(int x, int y)
		{
			return (new CellChunk(x, y), false);
		}

		public CellChunk GetChunk(int x, int y)
		{
			return new CellChunk(x, y);
		}
	}
}
