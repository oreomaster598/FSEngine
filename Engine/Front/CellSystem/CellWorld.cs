using FSEngine.Concurrency;
using FSEngine.GFX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FSEngine.CellSystem

{

	public unsafe class CellWorld
	{
		public Int32 frame = 0;
		public static Int32 CellsUpdated = 0;
		public UInt32 height;
		public UInt32 width;
		public const UInt16 chunk_s = 60;

		public ChunkCache cache = new ChunkCache();

		public TSBitmap front_buffer;
		internal TSBitmap back_buffer;


		public static bool ShowChunks = false;
		public static bool ShowDirtyRect = false;
		internal IChunkManager chunkProvider;
		public Vector2 PixelOffset = new Vector2(360, 360);
		public UInt32 BufferChunks = 5;
		public static Boolean SleepingChunks = false;
		public Boolean SkipCellMode = false;
		public Random rng = new Random();
		CellRasterizer rasterizer;

		public CellChunk Hover = null;


		public static TSBitmap lights = new TSBitmap(720, 480);
		public Vector2 COrigin
		{
			get
			{
				return new Vector2((Int32)(PixelOffset.X / chunk_s), (Int32)(PixelOffset.Y / chunk_s));
			}
			set
			{
				PixelOffset = new Vector2(value.X * chunk_s, value.Y * chunk_s);
			}
		}
		public CellWorld(UInt32 width, UInt32 height, IChunkManager chunkProvider, CellRasterizer rasterizer)
        {
            this.height = height;
            this.width = width;
            this.chunkProvider = chunkProvider;
            front_buffer = new TSBitmap(width * chunk_s, height * chunk_s);
            back_buffer = new TSBitmap(width * chunk_s, height * chunk_s);
            gfx = new CPUGraphics(back_buffer);
            //Physics.CollisionMap = new short[width * chunk_s, height * chunk_s];
            CalculateScale();
            this.rasterizer = rasterizer;
			//ForceAll();

		}
		public CellWorld(UInt32 width, UInt32 height, IChunkManager chunkProvider)
		{
			this.height = height;
			this.width = width;
			this.chunkProvider = chunkProvider;
			front_buffer = new TSBitmap(width * chunk_s, height * chunk_s);
			back_buffer = new TSBitmap(width * chunk_s, height * chunk_s);
			gfx = new CPUGraphics(back_buffer);
			//Physics.CollisionMap = new short[width * chunk_s, height * chunk_s];
			CalculateScale();
			this.rasterizer = new CellRasterizer();
			//ForceAll();

		}

		private Vector2 offset_back = new Vector2(360, 360);
		public void SetPixelOffset(Vector2 offset)
		{
			offset_back = offset;
		}
		public Vector2 scale = new Vector2(2,2);
		public Vector2 MousePosition = Vector2.Zero;
		public void CalculateScale()
        {
			scale = new Vector2(Program.window.Width, Program.window.Height) / new Vector2(Window.width, Window.height);
        }
		public Vector2 ToWorldPosition(int x, int y)
		{
			return Vector2.Clamp((new Vector2(x, y) / scale) + PixelOffset, Vector2.Zero, new Vector2(PixelOffset.X + (chunk_s * width), PixelOffset.Y + (chunk_s * height)));
		}
		public Vector2 ToScreenPosition(int x, int y)
		{
			return new Vector2(x - PixelOffset.X, y - PixelOffset.Y);
		}
		public Vector2 ToWorldSize(int x, int y)
		{
			return (new Vector2(x, y) / scale);
		}
		public Rectangle ToWorldPosition(Rectangle rect)
		{
			Vector2 pos = ToWorldPosition(rect.X, rect.Y);
			Vector2 size = ToWorldSize(rect.Width, rect.Height);

			return new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
		}

		public static int SkipRate = 1;
		public static Int16 HeatEffectMultiplier = 1;

		public Vector2 ToScreenPos(Vector2 pos)
		{
			return pos - PixelOffset;

		}
		public void SwapBuffers()
		{
			back_buffer.CopyTo(front_buffer);
			back_buffer.Clear();
			PixelOffset = offset_back;
		}
		public CPUGraphics gfx;
		GFX.Color TransparentRed = new GFX.Color(100, 255, 0, 0);
		GFX.Color TransparentGreen = new GFX.Color(100, 0, 255, 0);

		public void Raster()
		{
			if (frame % SkipRate != 0)
				return;

			int ox = (int)COrigin.X;
			int oy = (int)COrigin.Y;
            Parallel.For(ox, ox + width + 1, (cx) =>
            //for (int cx = ox; cx < ox + width + 1; cx++)
			{
				for (int cy = oy; cy < oy + height + 1; cy++)
				{
					//CellChunk c = GetChunk((int)cx * chunk_s, cy * chunk_s);

					CellChunk c = null;
					Vector2 cp = new Vector2(cx, cy);
					int cox = (int)cx * chunk_s, coy = cy * chunk_s;

					if (cache.chunks.ContainsKey(cp))
						c = cache.chunks[cp];
					else
						continue;


					if (c.filledcells == 0)
						continue;
					for (Int32 y = 0; y != chunk_s; y++)
						for (Int32 x = 0; x != chunk_s; x++)
						{
							Cell cell = c.GetCell(x, y);

							Vector2 pos = new Vector2(x + cox, y + coy) - PixelOffset;
							
							uint tx = (UInt32)pos.X;
							uint ty = (UInt32)pos.Y;


							if (pos.X < 0 || pos.Y < 0 || pos.X > (width * chunk_s - 1) || pos.Y > (height * chunk_s - 1))
								continue;


			

							if (cell.type == 0)
							{
								//(x <= c.maxX && x >= c.minX) && (y <= c.maxY && y >= c.minY)
								int maxX = c.maxX - c.minX - 1;
								int maxY = c.maxY - c.minY - 1;
								int X = x - c.minX;
								int Y = y - c.minY;

								if(ShowChunks)
                                {
									if (ShowDirtyRect && (X >= 0 && X <= maxX && Y >= 0 && Y <= maxY) && ((X % maxX) == 0 || (Y % maxY) == 0))
									{
										back_buffer.SetPixel(tx, ty, GFX.Color.Red);
									}
									else if(ShowDirtyRect && (x > c.minX && x < c.maxX) && (y > c.minY && y < c.maxY))
									{
										back_buffer.SetPixel(tx, ty, TransparentRed);
									}
									else if ((x % (chunk_s - 1) == 0) || (y % (chunk_s - 1) == 0))
									{
										back_buffer.SetPixel(tx, ty, GFX.Color.Green);
									}
									else
									{
										back_buffer.SetPixel(tx, ty, TransparentGreen);
									}
                                }
	
								//else
								//	back_buffer.SetPixel(tx, ty, CellRasterizer.RasterizeWall(tx, ty, c.biome));
								continue;
							}
							back_buffer.SetPixel(tx, ty, rasterizer.RasterizeCell(cell, (int)cx * chunk_s + x, cy * chunk_s + y,tx,ty));
							/*if (ShowChunks && (x <= c.maxX && x >= c.minXw) && (y <= c.maxY && y >= c.minYw))
							{
								back_buffer.MulPixel(tx, ty, 1.0f, 0.5f, 0.5f);
							}*/
						}
				}
			});
			SwapBuffers();
		}

		public static int cc = 0;
		public static int sc = 0;
		///Random double for random stuff
		public double random = 0;
		public bool simulate = true;
		private void UpdateVariables()
        {
			CalculateScale();
			random = Game.rng.NextDouble();
			frame++;
			CellsUpdated = 0;
			MousePosition = ToWorldPosition((int)Mouse.CursorPos.X, (int)Mouse.CursorPos.Y);
			step_start = Time.sw.ElapsedMilliseconds;
		}
		int phase = 0;
		long step_start = 0;
		/// <summary>
		/// Step The Cell Simulation.
		/// </summary>
		/// <typeparam name="T">Chunk Worker</typeparam>
		/// <param name="fps">Simulation FPS Won't exceed this value.</param>
		public void Step<T>(long fps = 30) where T : ChunkWorker
		{
			if (simulate) 
			{ 
				long min_time_ms = 1000 / fps;

				if (Time.sw.ElapsedMilliseconds - step_start < min_time_ms)
					return;
			}

			UpdateVariables();
			//PullAll();
			//cache.LoadAll(this);
			cache.chunks.Cycle((int)COrigin.X - (int)BufferChunks, (int)COrigin.Y - (int)BufferChunks);
			CellChunk[,] chunk = cache.chunks.chunks;

			int processed = 0;
			int added = 0;


			//int skip = -1;
			//if(false && Game.rng.Next(0, 10) == 0)
            //{
			//	skip = Game.rng.Next(0, 4);
			//}
			if (simulate)
			for (int i = 0; i < 4; i++)
			{
				//if (i == skip)
				//	continue;

				foreach (CellChunk c in chunk)
				{
					if (c == null)
						continue;

					if (c.x % 2 + (c.y % 2) * 2 == i)
					{
						added++;
						ThreadPool.QueueUserWorkItem((o) => 
						{
							if (!(c.Asleep && SleepingChunks))
							{	
								((ChunkWorker)Activator.CreateInstance(typeof(T), c, this)).Update();
								c.UpdateRect();
							}
							Interlocked.Increment(ref processed);
						});
					}
				}

				while (processed != added)
				{
					// Console.Write("\rPoccessed: {0}                         ", processed);
				}
				processed = 0;
			    added = 0;
			}

			lights.Clear();
			Raster();

			Vector2 pos = new Vector2((Int32)(MousePosition.X / chunk_s), (Int32)((int)MousePosition.Y / chunk_s));
			if (cache.chunks.ContainsKey(pos))
			{
				Hover = cache.chunks[pos];
			}
			else
				Hover = null;
		}
	

		public int TrueX(CellChunk chunk, Int32 pos)
			=> pos + chunk.x * chunk_s;

		public int TrueY(CellChunk chunk, Int32 pos)
			=> pos + chunk.y * chunk_s;

		public void Wake(Int32 x, Int32 y)
		{
			Vector2 pos = new Vector2((Int32)(x / chunk_s), (Int32)(y / chunk_s));
			if (cache.chunks.ContainsKey(pos))
				cache.chunks[pos].Asleep = false;
		}

		public void Notify(Int32 x, Int32 y)
		{
			Vector2 pos = new Vector2((Int32)(x / chunk_s), (Int32)(y / chunk_s));
			if (cache.chunks.ContainsKey(pos))
			{
				cache.chunks[pos].Asleep = false;
				cache.chunks[pos].KeepAlive(x % chunk_s, y % chunk_s);
			}

		}
		public Vector2 TruePosToChunkPos(int x, int y)
		{
			return new Vector2((Int32)(x / chunk_s), (Int32)(y / chunk_s));
		}
		public CellChunk GetChunk(Int32 x, Int32 y)
		{
			Vector2 pos = new Vector2((x / chunk_s), (y / chunk_s));
			if (cache.chunks.ContainsKey(pos))
			{
				return cache.chunks[pos];
			}
			CellChunk c = new CellChunk((Int32)pos.X, (Int32)pos.Y);
			if (cache.chunks.InBounds((Int32)pos.X, (Int32)pos.Y))
			cache.chunks.Add(pos, c);
			return c;
		}
		public CellChunk PeekChunk(Int32 x, Int32 y)
		{
			Vector2 pos = new Vector2((Int32)(x / chunk_s), (Int32)(y / chunk_s));
			if (cache.chunks.ContainsKey(pos))
			{
				return cache.chunks[pos];
			}
			return new CellChunk(x, y);
		}
		public void MoveCell(Int32 x, Int32 y, Int32 nx, Int32 ny)
		{
			CellChunk chunk = GetChunk(x, y);
			CellChunk chunk2 = GetChunk(nx, ny);

			Int32 mx = x % chunk_s;
			Int32 my = y % chunk_s;
			Int32 mnx = nx % chunk_s;
			Int32 mny = ny % chunk_s;

			Cell ce = chunk.GetCell(mx, my);

			chunk.Clear(mx, my);
			chunk2.SetCell(mnx, mny, ce);
		}

		public void MoveCell(CellChunk src, Int32 x, Int32 y, Int32 nx, Int32 ny)
		{
			CellChunk chunk2 = GetChunk(nx, ny);

			Int32 mnx = nx % chunk_s;
			Int32 mny = ny % chunk_s;

			Cell ce = src.GetCell(x, y);
			ce.frame = (uint)frame;
			ce.life++;
			src.Clear(x, y);
			chunk2.SetCell(mnx, mny, ce);
		}
		public bool IsEmpty(Int32 x, Int32 y)
		{
			if (!InBounds(x, y))
				return false;

			return GetChunk(x, y).IsEmpty(x % chunk_s, y % chunk_s);
		}
		public bool InBounds(Int32 x, Int32 y)
		{
			return cache.chunks.InBounds(x / chunk_s, y / chunk_s);
		}
		public void SetCell(Int32 x, Int32 y, Cell c)
		{
			GetChunk(x, y).SetCell(x % chunk_s, y % chunk_s, c);
		}
		public void SetCellDead(Int32 x, Int32 y, Cell c)
		{
			GetChunk(x, y).SetCellDead(x % chunk_s, y % chunk_s, c);
		}
		public bool TrySetCell(Int32 x, Int32 y, Cell c)
		{
			CellChunk cc = GetChunk(x, y);
			if(cc.IsEmpty(x % chunk_s, y % chunk_s))
            {
				cc.SetCell(x % chunk_s, y % chunk_s, c);
				return true;
            }
			return false;
		}
		public bool TrySetCellDead(Int32 x, Int32 y, Cell c)
		{
			CellChunk cc = GetChunk(x, y);
			if (cc.IsEmpty(x % chunk_s, y % chunk_s))
			{
				cc.SetCellDead(x % chunk_s, y % chunk_s, c);
				return true;
			}
			return false;
		}
		public void SetCell(Int32 x, Int32 y, short id)
		{
			GetChunk(x, y).SetCell(x % chunk_s, y % chunk_s, Sampler.GetSampler(id).GetCell(x, y));
		}
		public void ChangeCell(Int32 x, Int32 y, short id)
		{
			Cell c = Sampler.GetSampler(id).GetCell(x, y);
			c.frame = (uint)frame;
			GetChunk(x, y).SetCell(x % chunk_s, y % chunk_s, c);
		}
		public void Clear(Int32 x, Int32 y)
		{
			GetChunk(x, y).Clear(x % chunk_s, y % chunk_s);
		}
		public void Darken(Int32 x, Int32 y, float factor)
		{
			Cell c = GetChunk(x, y).GetCell(x % chunk_s, y % chunk_s);
			c.R = (byte)(c.R * factor);
			c.G = (byte)(c.G * factor);
			c.B = (byte)(c.B * factor);
			GetChunk(x, y).SetCell(x % chunk_s, y % chunk_s, c);
		}
		public void PeekSetCell(Int32 x, Int32 y, Cell c)
		{
			PeekChunk(x, y).SetCell(x % chunk_s, y % chunk_s, c);
		}
		public Cell GetCell(Int32 x, Int32 y)
		{
			return GetChunk(x, y).GetCell(x % chunk_s, y % chunk_s);
		}
		public Cell PeekCell(Int32 x, Int32 y)
		{
			return PeekChunk(x, y).GetCell(x % chunk_s, y % chunk_s);
		}
	}
}
