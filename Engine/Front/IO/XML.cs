using FSEngine.GFX;
using FSEngine.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
namespace FSEngine.IO
{
    public static class XMLLoader
    {
        public static Parallax LoadParallax(Game game, string file)
        {
            Parallax parallax = new Parallax();
            bool sort = false;
            using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read))
            using (XmlReader reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if(reader.Name == "Layer")
                    {
                        string texture = reader.GetAttribute("texture");
                        float speed = float.Parse(reader.GetAttribute("speed"));


                        if (texture.StartsWith("./"))
                            texture = texture.Remove(0, 1).Insert(0, Path.GetDirectoryName(file));

                        parallax.AddLayer(game.resources.LoadTexture(texture), speed);
                    }
                    else if(reader.Name == "Parallax")
                    {
                        if(reader.AttributeCount > 0)
                        {
                            sort = bool.Parse(reader.GetAttribute("sort"));
                        }
                    }
                }
            }
            if (sort)
                parallax.Sort();
            return parallax;
        }
    }
}
