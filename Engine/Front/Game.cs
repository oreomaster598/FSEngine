using OpenTK.Input;

using System;
using System.Drawing;
using System.Numerics;

using FSEngine.CellSystem;
using FSEngine.OpenGL;
using FSEngine.UI;
using ImGuiNET;
using Graphics = FSEngine.OpenGL.Graphics;
using System.IO;
using FSEngine.CellSystem.Effects.Particles;
using FSEngine.CellSystem.Effects;
using FSEngine.GFX;
using FSEngine.Front.CellSystem.Events;
using FSEngine.Audio;
using OpenTK.Graphics.OpenGL;
using FSEngine.Concurrency;
using FSEngine.QoL;
using Box2DSharp.Dynamics;

namespace FSEngine
{

    public class Game
    {


        public static Random rng = new Random();
        public CellWorld cellworld;
        public ResourceFactory resources;
        public Statistics stats;
        public World world = new World(new Vector2(0, 10));
        public Game()
        {
            resources = new ResourceFactory(this);
            stats = new Statistics();
        }

        public void DrawStats()
        {
            ImGui.Begin("Stats");

            if(ImGui.CollapsingHeader("Resources"))
            {
                ImGui.Text($"{stats.Shaders} Shader(s)");
                ImGui.Text($"{stats.Sounds} Sound(s)");
                ImGui.Text($"{stats.Models} Model(s)");
                ImGui.Text($"{stats.Textures} Texture(s)");
                ImGui.Text($"{stats.Files} File(s)");
            }

            if (ImGui.CollapsingHeader("Box2D"))
            {
                ImGui.Text($"{stats.b2dBodies} Bodies");
                ImGui.Text($"{stats.b2dJoints} Joint(s)");
                ImGui.Text($"{stats.b2dTriangles} Collider Triangle(s)");
            }
            ImGui.End();
        }

        public virtual void DrawUI()
        {
           
        }


        public virtual void OnKey(KeyboardState state, ButtonState buttonState)
        {

        }
           

        public virtual void PreloadContent()
        {
           
        }

        public virtual void MouseWheel()
        {

          
        }

        public virtual void UnloadContent()
        {

        }

        public virtual void LoadContent()
        {
           
        }

        public virtual void Draw()
        {
           
        }



        public virtual void Update()
        {

            
        }

    }
}