using FSEngine.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.GFX
{
    public class Parallax
    {
        public List<ParallaxLayer> layers = new List<ParallaxLayer>();
        public Parallax()
        {

        }
        public void Move(float x, float res)
        {
            foreach (ParallaxLayer layer in layers)
            {
                layer.offset = ((x * layer.speed) % res) / res;
            }
        }
        public void Sort()
        {
            layers.Sort((x, y) => x.speed.CompareTo(y.speed));
        }
        public void AddLayer(ParallaxLayer layer)
        {
            layers.Add(layer);
        }
        public void AddLayer(Texture tex, float speed)
        {
            layers.Add(new ParallaxLayer(speed, tex)) ;
        }
    }
    public class ParallaxLayer
    {
        public float offset;
        public float speed;
        public Texture texture;

        public ParallaxLayer(float speed, Texture texture)
        {
            this.speed = speed;
            this.texture = texture;
        }
    }
}
