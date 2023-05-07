using FSEngine.GFX;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.OpenGL
{
    public static class Graphics
    {
        public static Model CurrentMDL;
        public static Shader CurrentSDR;
        public static Shader TextureShader;
        public static Shader ParallaxShader;
        public static Camera Camera;
        public static void Begin(Model model, Camera camera)
        {
            Camera = camera;
            CurrentMDL = model;
            CurrentMDL.Bind();
        }
        public static void End()
        {
            CurrentMDL.Unbind();
            if(CurrentSDR != null)
                CurrentSDR.Unbind();
        }
        public static void UseShader(Shader shader)
        {
            CurrentSDR = shader;
            CurrentSDR.Bind();
        }

        public static void UseTextureShader()
        {
            CurrentSDR = TextureShader;
            CurrentSDR.Bind();
        }

        public static void Draw(Transform transform)
        {
            CurrentSDR.SetMatrix4x4("model", transform.GetModelMatrix());
            CurrentSDR.SetMatrix4x4("projection", Camera.GetProjectionMatrix());

            CurrentMDL.Draw();
        }
        public static void Draw(Transform transform, Texture texture)
        {
            texture.Bind();

            CurrentSDR.SetMatrix4x4("model", transform.GetModelMatrix());
            CurrentSDR.SetMatrix4x4("projection", Camera.GetProjectionMatrix());

            CurrentMDL.Draw();

            texture.Unbind();
        }
        public static void Draw(Transform transform, Texture texture, Camera cam)
        {
            texture.Bind();

            CurrentSDR.SetMatrix4x4("model", transform.GetModelMatrix());
            CurrentSDR.SetMatrix4x4("projection", cam.GetProjectionMatrix());

            CurrentMDL.Draw();

            texture.Unbind();
        }
        public static void DrawParallax(Transform transform, Parallax parallax)
        {
            
            ParallaxShader.Bind();



            ParallaxShader.SetMatrix4x4("model", transform.GetModelMatrix());
            ParallaxShader.SetMatrix4x4("projection", Camera.GetProjectionMatrix());

            foreach(ParallaxLayer layer in parallax.layers)
            {
                layer.texture.Bind();
                ParallaxShader.SetFloat("pos", layer.offset);
                CurrentMDL.Draw();
            }


            if(CurrentSDR != null)
                CurrentSDR.Bind();
        }
        public static void DrawParallax(Transform transform, Parallax parallax, int layer)
        {

            ParallaxShader.Bind();



            ParallaxShader.SetMatrix4x4("model", transform.GetModelMatrix());
            ParallaxShader.SetMatrix4x4("projection", Camera.GetProjectionMatrix());

            ParallaxLayer _layer = parallax.layers[layer];
            {
                _layer.texture.Bind();
                ParallaxShader.SetFloat("pos", _layer.offset);
                CurrentMDL.Draw();
            }


            if (CurrentSDR != null)
                CurrentSDR.Bind();
        }

        public static void Draw(Transform transform, Texture texture, Shader shader)
        {
            texture.Bind();
            shader.Bind();

            CurrentSDR.SetMatrix4x4("model", transform.GetModelMatrix());
            CurrentSDR.SetMatrix4x4("projection", Camera.GetProjectionMatrix());

            CurrentMDL.Draw();
            if (CurrentSDR != null)
                CurrentSDR.Bind();
            texture.Unbind();
   

        }
    }
}
