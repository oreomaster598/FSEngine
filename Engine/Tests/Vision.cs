using FSEngine.CellSystem;
using FSEngine.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.Tests
{
	public class NormalVision : CellRasterizer
	{
		int i = 0;
		uint magic = 432;
		
		public override void Init()
		{
			i++;
			if (i == 5)
			{
				magic = FastRandi();
				i = 0;
			}
		}

		public static Color KelvinToRGB(double kelvin)
        {

			double temp = kelvin / 100;

			double red, green, blue;

			if (temp <= 66)
			{

				red = 255;

				green = temp;
				green = 99.4708025861 * Math.Log(green) - 161.1195681661;


				if (temp <= 19)
				{

					blue = 0;

				}
				else
				{

					blue = temp - 10;
					blue = 138.5177312231 * Math.Log(blue) - 305.0447927307;

				}

			}
			else
			{

				red = temp - 60;
				red = 329.698727446 * Math.Pow(red, -0.1332047592);

				green = temp - 60;
				green = 288.1221695283 * Math.Pow(green, -0.0755148492);

				blue = 255;

			}
			return Color.FromArgb((int)red, (int)green, (int)blue);

		}
		private Color blackBodyColor(double temp)
		{
			float x = (float)(temp / 1000.0);
			float x2 = x * x;
			float x3 = x2 * x;
			float x4 = x3 * x;
			float x5 = x4 * x;

			float R, G, B = 0f;

			// red
			if (temp <= 6600)
				R = 1f;
			else
				R = 0.0002889f * x5 - 0.01258f * x4 + 0.2148f * x3 - 1.776f * x2 + 6.907f * x - 8.723f;

			// green
			if (temp <= 6600)
				G = -4.593e-05f * x5 + 0.001424f * x4 - 0.01489f * x3 + 0.0498f * x2 + 0.1669f * x - 0.1653f;
			else
				G = -1.308e-07f * x5 + 1.745e-05f * x4 - 0.0009116f * x3 + 0.02348f * x2 - 0.3048f * x + 2.159f;

			// blue
			if (temp <= 2000f)
				B = 0f;
			else if (temp < 6600f)
				B = 1.764e-05f * x5 + 0.0003575f * x4 - 0.01554f * x3 + 0.1549f * x2 - 0.3682f * x + 0.2386f;
			else
				B = 1f;

			return Color.FromArgb(1f, R, G, B);
		}
		public override Color RasterizeCell(Cell cell, int x, int y, uint sx, uint sy)
		{
			return Color.FromArgb(cell.A, cell.R, cell.G, cell.B);
            Color c = Color.FromArgb(cell.A, cell.R, cell.G, cell.B);
			if (cell.type == -1)//barrier
			{
				return Color.FromArgb(64, 255, 0, 0);
			}
			else if (cell.type == (short)MaterialType.Ember)
			{
				c = Color.FromHSL(25, 1, (1 - cell.life / 80f) * 0.85f);
				c.A = 128;
				CellWorld.lights.SetPixel(sx, sy, Color.FromArgb(255, c.R, c.G, c.B));
			}
			else if (cell.type == (short)MaterialType.Fire)
			{
				c = Color.FromHSL(25, 1, (1 - cell.life / 300f) * 0.85f);
				c.A = 128;
				CellWorld.lights.SetPixel(sx, sy, Color.FromArgb(255, c.R, c.G, c.B));
			}
			else if (/*cell.type == (short)MaterialType.Lava ||*/ cell.type == (short)MaterialType.Acid || cell.type == (short)MaterialType.Toxic_Gas)
			{
				CellWorld.lights.SetPixel(sx, sy, Color.FromArgb(255, cell.R, cell.G, cell.B));
			}
			if (cell.heat != 0)
			{
		
				if (cell.heat > 1750)
				{ 
					Color heat =  blackBodyColor(cell.heat / 2);
					CellWorld.lights.SetPixel(sx, sy, Color.FromArgb(255, heat.R, heat.G, heat.B));
					c.R = (byte)(c.R * .6 + heat.R * .4);
					c.G = (byte)(c.G * .6 + heat.G * .4);
					c.B = (byte)(c.B * .6 + heat.B * .4);
					//c.A = 255;
				}
			}
			return Color.FromArgb(c.A, c.R, c.G, c.B);
		}
	}
	public class ThermalVision : CellRasterizer
	{

		public static Color waveLengthToRGB(double Wavelength)
		{
			double factor;
			double Red, Green, Blue;

			if ((Wavelength >= 380) && (Wavelength < 440))
			{
				Red = -(Wavelength - 440) / (440 - 380);
				Green = 0.0;
				Blue = 1.0;
			}
			else if ((Wavelength >= 440) && (Wavelength < 490))
			{
				Red = 0.0;
				Green = (Wavelength - 440) / (490 - 440);
				Blue = 1.0;
			}
			else if ((Wavelength >= 490) && (Wavelength < 510))
			{
				Red = 0.0;
				Green = 1.0;
				Blue = -(Wavelength - 510) / (510 - 490);
			}
			else if ((Wavelength >= 510) && (Wavelength < 580))
			{
				Red = (Wavelength - 510) / (580 - 510);
				Green = 1.0;
				Blue = 0.0;
			}
			else if ((Wavelength >= 580) && (Wavelength < 645))
			{
				Red = 1.0;
				Green = -(Wavelength - 645) / (645 - 580);
				Blue = 0.0;
			}
			else if ((Wavelength >= 645) && (Wavelength < 781))
			{
				Red = 1.0;
				Green = 0.0;
				Blue = 0.0;
			}
			else
			{
				Red = 0.0;
				Green = 0.0;
				Blue = 0.0;
			}

			// Let the intensity fall off near the vision limits

			if ((Wavelength >= 380) && (Wavelength < 420))
			{
				factor = 0.3 + 0.7 * (Wavelength - 380) / (420 - 380);
			}
			else if ((Wavelength >= 420) && (Wavelength < 701))
			{
				factor = 1.0;
			}
			else if ((Wavelength >= 701) && (Wavelength < 781))
			{
				factor = 0.3 + 0.7 * (780 - Wavelength) / (780 - 700);
			}
			else
			{
				factor = 0.0;
			}
			return Color.FromArgb(1f, (float)Red * (float)factor, (float)Green * (float)factor, (float)Blue * (float)factor);
		}
		public override Color RasterizeCell(Cell cell, int x, int y, uint sx, uint sy)
		{
			if (cell.heat == 0)
				cell.heat = 1;
			double wave = ((double)cell.heat / 32767d) * 300d + 450d;
			if (wave < 380)
				wave = 380;
			return waveLengthToRGB(wave);
		}
	}
}
