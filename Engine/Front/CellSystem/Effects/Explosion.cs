using FSEngine;
using FSEngine.CellSystem.Effects.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.CellSystem.Effects
{
	public class Explosion
	{
		public System.Drawing.Color color;
		public int strength;
		public int radius;
		public float falloff_factor;
		public short cell = 0;
		public Explosion(System.Drawing.Color color, Int32 strength, Int32 radius, Single falloff_factor = 1.0f, short cell = 0)
		{
			this.color = color;
			this.strength = strength;
			this.radius = radius;
			this.falloff_factor = falloff_factor;
			this.cell = cell;
		}
		public void Explode(CellWorld world, float x, float y)
		{
			//Window.game.paused = true;
			float res = radius / 10;
			for (float ang = 0; ang < 360 * res; ang++)
			{
				float cos = ((float)Math.Cos(ang / res) * (float)radius);
				float sin = ((float)Math.Sin(ang / res) * (float)radius);

				float dist = (float)Math.Sqrt(Math.Pow(cos, 2) + Math.Pow(sin, 2));

				float vx = cos / dist;
				float vy = sin / dist;
				float px = x;
				float py = y;

				bool darken = false;
				float temp_strength = strength;
				//float dark = 0.8f;
				for (int i = 0; i < dist; i++)
				{
					px += vx;
					py += vy;
					int destx = (int)Math.Abs(px);
					int desty = (int)Math.Abs(py);
					float dark = (float)i / dist;
					if (darken)
					{
						world.Darken(destx, desty, dark);
						continue;
					}
					Cell c = world.PeekCell(destx, desty);
					Material m = Materials.Get(c.type);
					if (c.type == 0 || c.type == cell)
						m.Hardness = 0;
					temp_strength -= (float)m.Hardness * falloff_factor;

					if (m.Hardness > (int)temp_strength)
					{
						if (((int)ang) % 1 == 0)
						{
							darken = true;
							world.Darken(destx, desty, dark);
							continue;
						}
						else
							continue;

						//dark += darkinc;
					}

					world.SetCell(destx, desty, cell);
				}
			}
			//Vector2 sp = world.ToScreenPosition((int)x, (int)y);
			//ParticleEngine.AddEmitter(new Emitter(new Particle(false, (int)sp.X, (int)sp.Y, 0f, 0f, 60, false, color.A, color.R, color.G, color.B), 50));
			//Window.game.paused = false;
		}
		public static Explosion EffectOnly(System.Drawing.Color color, Int32 radius)
		{
			return new Explosion(color, -1, radius, 0);
		}
	}
}
