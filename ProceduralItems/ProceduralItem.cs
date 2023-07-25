using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralItems
{
    public struct ColorF
    {
        public float r, g, b;

        public ColorF(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }
    public class Segment
    {
        public Bitmap image;
        public int to_x, to_y, from_x, from_y;
        public string[] words;

        public static Segment Load(string file)
        {

            Segment s = new Segment();
            string[] stuff = Path.GetFileNameWithoutExtension(file).Split('_');
            s.to_x = Convert.ToInt32(stuff[1]);
            s.to_y = Convert.ToInt32(stuff[2]);
            s.from_x = Convert.ToInt32(stuff[3]);
            s.from_y = Convert.ToInt32(stuff[4]);
            s.image = new Bitmap(file);
            if (stuff[0].Contains('-'))
                s.words = stuff[0].Split('-');
            else
                s.words = new string[] { stuff[0] };
            return s;
        }
    }

    public class Part
    {
        public List<Segment> parts = new List<Segment>();
        public List<ColorF> materials = new List<ColorF>();

        public static Part Load(string dir)
        {
            Part p = new Part();
            ProceduralItem.LoadColors(ref p.materials, Path.Combine(dir, "materials.txt"));
            foreach (string file in Directory.GetFiles(dir))
            {
                if (file.EndsWith(".png"))
                    p.parts.Add(Segment.Load(file));
            }
            return p;
        }
    }

    public static class ProceduralName
    {
        static string[] end_parts = { "ium", "ite", "th", "che", "sh", "sion", "tion", "te", "the", "ck" };
        static string[] start_parts = {"qu", "ch", "th", "sh"};
        static char[] can_double = { 'o', 'e', 'l'};
        static char[] in_middle = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'l', 'm', 'n', 'o', 'p', 'r', 's', 't', 'u', 'v' };
        static string[] middle_parts = { "ea" };
        static char[] chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        static char[] on_end = { 'k','t','l','f','e'};
        private static string[] VOWEL_DICTIONARY = { "a", "a", "a", "e", "e", "e", "e", "i", "i", "u", "u", "o", "o", "y" };
        private static string[] CONSONANT_DICTIONARY = { "w", "r", "t", "p", "s", "d", "f", "x", "g", "h", "j", "k", "l", "z", "c", "v", "b", "n", "m", "sh", "ch", "ph", "th", "qu" };

        public static string Upper(string s)
        {
            return s.Remove(0, 1).Insert(0, char.ToUpper(s[0]).ToString());
        }

        public static string Construct(int seed = 0)
        {
            if (seed == 0)
                seed = new Random().Next();


            Random random = new Random(seed);

            string word = "";

            if (random.Next(0, 2) == 0)
                word += start_parts[random.Next(0, start_parts.Length)];
            else
            {
                word += chars[random.Next(0, chars.Length)];
                if (word.EndsWith("q"))
                    word += "u";

                if (random.Next(0, 3) == 0 && can_double.Contains(word.Last()))
                    word += word.Last();
            }
            if (random.Next(0, 2) == 0)
            {
                word += middle_parts[random.Next(0, middle_parts.Length)];
            }
            else
            {
                int cc = 0;
                for (int i = 0; i < random.Next(0, 8); i++)
                {
                    char c = in_middle[random.Next(0, in_middle.Length)];
                    if (CONSONANT_DICTIONARY.Contains(c.ToString()))
                        cc++;
                    else
                        cc = 0;

                    if (cc > 1)
                        c = VOWEL_DICTIONARY[random.Next(VOWEL_DICTIONARY.Length)].ToCharArray()[0];
                    word += c;
                    if (random.Next(0, 3) == 0 && can_double.Contains(c))
                        word += c;


                }
            }
            if (random.Next(0, 2) == 0)
            {
                word += end_parts[random.Next(0, end_parts.Length)];
            }
            else
            {
                word += on_end[random.Next(0, on_end.Length)];
                if (random.Next(0, 3) == 0 && can_double.Contains(word.Last()))
                    word += word.Last();
            }
            if (word.Length < 4)
                return Construct(seed + 1);

            word = Upper(word);
            return word;
        }
    
       
    }
    public static class ProceduralItem
    {

        public static Part shaft_part = new Part();
        public static Part guard_part = new Part();
        public static Part pommel_part = new Part();
        public static Part blade_part = new Part();
        public static void LoadColors(ref List<ColorF> colors, string file)
        {
            string[] txt = File.ReadAllLines(file);

            foreach(string s in txt)
            {
                if(!string.IsNullOrEmpty(s))
                {
                    string[] v = s.Split(',');

                    ColorF c = new ColorF();
                    c.r = float.Parse(v[0]);
                    c.g = float.Parse(v[1]);
                    c.b = float.Parse(v[2]);
                    colors.Add(c);
                }
            }
        }

        public static void LoadParts()
        {
            blade_part = Part.Load(Path.Combine(Directory.GetCurrentDirectory(), "Textures/Blades"));
            shaft_part = Part.Load(Path.Combine(Directory.GetCurrentDirectory(), "Textures/Shaft"));
            pommel_part = Part.Load(Path.Combine(Directory.GetCurrentDirectory(), "Textures/Pommel"));
            guard_part = Part.Load(Path.Combine(Directory.GetCurrentDirectory(), "Textures/Guard"));
        }

        public static void DrawImage(this Graphics g, Bitmap image, int x, int y, ColorF c)
        {
            ImageAttributes imageAttributes = new ImageAttributes();
            int width = image.Width;
            int height = image.Height;

            float[][] colorMatrixElements = {
   new float[] { c.r,  0,  0,  0, 0},        // red scaling factor of 2
   new float[] {0, c.g,  0,  0, 0},        // green scaling factor of 1
   new float[] {0,  0, c.b,  0, 0},        // blue scaling factor of 1
   new float[] {0,  0,  0,  1, 0},        // alpha scaling factor of 1
   new float[] {0, 0, 0, 0, 0}};    // three translations of 0.2

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);

            imageAttributes.SetColorMatrix(
               colorMatrix,
               ColorMatrixFlag.Default,
               ColorAdjustType.Bitmap);

            g.DrawImage(
               image,
               new Rectangle(x, y, width, height),  // destination rectangle 
               0, 0,        // upper-left corner of source rectangle 
               width,       // width of source rectangle
               height,      // height of source rectangle
               GraphicsUnit.Pixel,
               imageAttributes);
        }

        public static string[] AssembleNew(Bitmap surface, ref int seed)
        {
            if (seed == 0)
                seed = new Random().Next();


            Random random = new Random(seed);

            Segment pommel = pommel_part.parts[random.Next(0, pommel_part.parts.Count)];
            Segment guard = guard_part.parts[random.Next(0, guard_part.parts.Count)];
            Segment shaft = shaft_part.parts[random.Next(0, shaft_part.parts.Count)];
            Segment blade = blade_part.parts[random.Next(0, blade_part.parts.Count)];

            ColorF pommel_mat = pommel_part.materials[random.Next(0, pommel_part.materials.Count)];
            ColorF guard_mat = guard_part.materials[random.Next(0, guard_part.materials.Count)];
            ColorF shaft_mat = shaft_part.materials[random.Next(0, shaft_part.materials.Count)];
            ColorF blade_mat = blade_part.materials[random.Next(0, blade_part.materials.Count)];



            using (Graphics g = Graphics.FromImage(surface))
            {
                g.Clear(Color.Transparent);
                int x = 0, y = 48;


                x -= pommel.from_x;
                y += pommel.from_y;
                g.DrawImage(pommel.image,x, y, pommel_mat);
                x += pommel.to_x - shaft.from_x;
                y -= pommel.to_y - shaft.from_y;
                g.DrawImage(shaft.image, x, y, shaft_mat);
                x += shaft.to_x - guard.from_x;
                y -= shaft.to_y - guard.from_y;
                g.DrawImage(guard.image, x, y, guard_mat);
                x += guard.to_x - blade.from_x;
                y -= guard.to_y - blade.from_y;
                g.DrawImage(blade.image, x, y, blade_mat);

                //Console.WriteLine("Size Guess: " + Math.Abs(x + blade.to_x+1) + "," + Math.Abs(y - blade.to_y-1));
            }
            return blade.words;
        }
    }
}
