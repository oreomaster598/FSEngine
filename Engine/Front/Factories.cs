using FSEngine.Audio;
using FSEngine.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine
{
    public class ResourceFactory
    {
        public List<IDisposable> disposables = new List<IDisposable>();

        public static Model Quad;

        private Game game;
        public ResourceFactory(Game game)
        {
            this.game = game;
        }
        public Shader LoadShader(string file)
        {
            Shader sdr = Shader.Compile(File.ReadAllText(file) + "\0");
            disposables.Add(sdr);
            game.stats.Shaders++;
            return sdr;
        }
        public Shader CompileShader(string glsl)
        {
            Shader sdr = Shader.Compile(glsl + "\0");
            disposables.Add(sdr);
            game.stats.Shaders++;
            return sdr;
        }
        public Shader CompileShader(string glsl, string vert)
        {
            Shader sdr = Shader.Compile(glsl, vert);
            disposables.Add(sdr);
            game.stats.Shaders++;
            return sdr;
        }

        public Texture LoadTexture(string path)
        {
            Texture tex = Texture.FromFile(path);
            disposables.Add(tex);
            game.stats.Textures++;
            return tex;
        }
        public Sound LoadSound(string path)
        {
            Sound s = Sound.FromFile(path);
            disposables.Add(s);
            game.stats.Sounds++;
            return s;
        }

        public Model FromVertices(float[] verts)
        {
            Model model = Model.FromVertices(verts);
            disposables.Add(model);
            game.stats.Models++;
            return model;
        }

        public void LoadPrimitives()
        {
           Quad = FromVertices(Model.Quad);
        }

        public void Unload()
        {
            disposables.Clear();
            Console.WriteLine($"Unloaded {game.stats.Models} model(s).");
            Console.WriteLine($"Unloaded {game.stats.Shaders} shader(s).");
            Console.WriteLine($"Unloaded {game.stats.Textures} texture(s).");
            Console.WriteLine($"Unloaded {game.stats.Sounds} sound(s).");
        }
    }
}
