using OpenTK.Input;

using System;
using System.Numerics;
using FSEngine.CellSystem;
using FSEngine.OpenGL;
using ImGuiNET;
using Graphics = FSEngine.OpenGL.Graphics;
using System.IO;
using FSEngine.CellSystem.Effects.Particles;
using FSEngine.GFX;
using FSEngine.Front.CellSystem.Events;
using OpenTK.Graphics.OpenGL;
using FSEngine;


namespace FSEngine.Editor
{
 
    public class FSEditor : Game
    {

        Shader post;
        Color ambient_light = Color.FromArgb(160, 160, 160);

        EditorRasterizer rasterizer = new EditorRasterizer();
        B2DDebug b2dd = new B2DDebug();

        Vector2 world_editor_pos;
        Vector2 world_editor_scale;

        float bar_size = 0;

        public FSEditor() : base()
        {

        }

        public override void DrawUI()
        {
            bar_size = ImGui.GetIO().Fonts.Fonts[0].FontSize + ImGui.GetStyle().FramePadding.Y * 2;

            ImGui.Begin("World Edit");

            Vector2 sz = ImGui.GetWindowSize();
            if (sz.X / 1.5f != sz.Y)
            {

                    ImGui.SetWindowSize(new Vector2(sz.X, sz.X / 1.5f));
            }
            world_editor_pos = ImGui.GetWindowPos() + new Vector2(0, bar_size);
            world_editor_scale = ImGui.GetMainViewport().Size / (ImGui.GetWindowSize() - new Vector2(0, bar_size));

            ImGui.Image((IntPtr)cellworld.front_buffer.glTexture.id, ImGui.GetContentRegionAvail());

            ImGui.End();
        }


        public override void PreloadContent()
        {
            Materials.Add(new Material() { Hardness = 20, Process = (byte)0, Density = 80, CollisionData = 255, Flammability = 0, CorrosionResist = 15 }, "Content/Samples/sand.png");
            Materials.Add(new Material() { Hardness = 100, Process = (byte)0, Density = 100, CollisionData = 255, CanMelt = true, Flammability = 0, CorrosionResist = 5 }, "Content/Samples/iron.png");
            Materials.Add(new Material() { DispersionRate = 5, Hardness = 0, Process = (byte)0, Density = 25, CollisionData = 0, CorrosionResist = 1000 }, "Content/Samples/water.png");
            Materials.Add(new Material() { DispersionRate = 2, Hardness = 0, Process = (byte)0, Density = 15, CollisionData = 0, Corrosive = 10, CorrosionResist = 1000 }, "Content/Samples/acid.png");
            Materials.Add(new Material() { DispersionRate = 2, Hardness = 0, Process = (byte)0, Density = 5, CollisionData = 0, Life = 1000, CorrosionResist = 1000 }, "Content/Samples/toxic_gas.png");
            Materials.Add(new Material() { Hardness = 0, Process = (byte)0, Density = 50, CollisionData = 255, CorrosionResist = 1000, StaticHeat = 1800 }, "Content/Samples/lava.png");
            Materials.Add(new Material() { Hardness = 75, Process = (byte)0, Density = 100, CollisionData = 255, CorrosionResist = 50, Flammability = 0, CanMelt = true }, "Content/Samples/glass.png");
            Materials.Add(new Material() { Hardness = 5, Process = (byte)0, Density = 75, CollisionData = 255, CorrosionResist = 1000 }, "Content/Samples/ice.png");
            Materials.Add(new Material() { DispersionRate = 3, Hardness = 0, Process = (byte)0, Density = 10, CollisionData = 255, CorrosionResist = 1000, StaticHeat = -16000 }, "Content/Samples/liquid_air.png");
            Materials.Add(new Material() { DispersionRate = 3, Hardness = 0, Process = (byte)0, Density = 70, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/suspicious_liquid.png");
            Materials.Add(new Material() { Hardness = 20, Process = (byte)0, Density = 100, CollisionData = 0, Flammability = 95, CorrosionResist = 60 }, "Content/Samples/wood.png");
            Materials.Add(new Material() { Hardness = 1000, Process = (byte)0, Density = 1000, CollisionData = 255, CorrosionResist = 9 }, "Content/Samples/exotic_matter.png");// up
            Materials.Add(new Material() { Hardness = 2, Process = (byte)0, Density = 100, CollisionData = 255, Flammability = 0, CorrosionResist = 10000 }, "Content/Samples/rust.png");
            Materials.Add(new Material() { Hardness = 10, Process = (byte)0, Density = 200, CollisionData = 255, CanMelt = true, Flammability = 0, CorrosionResist = 10000 }, "Content/Samples/gold.png");
            Materials.Add(new Material() { Hardness = 1000, Process = (byte)0, Density = 10000, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/m_stone.png");
            Materials.Add(new Material() { Hardness = 50, Process = (byte)0, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/rock.png");
            Materials.Add(new Material() { Hardness = 0, Process = (byte)0, Density = 10, CollisionData = 0, Flammability = 50, CorrosionResist = 10000 }, "Content/Samples/grass.png");
            Materials.Add(new Material() { Hardness = 0, Process = (byte)0, Density = 25, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/dirt.png");
            Materials.Add(new Material() { Hardness = 100000, Process = (byte)0, Density = 0, Life = 250, CollisionData = 0, CorrosionResist = 10000 }, "Content/Samples/lava.png");
            Materials.Add(new Material() { Hardness = 80, Process = (byte)0, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/sandstone.png");
            Materials.Add(new Material() { Hardness = 20, Process = (byte)0, Density = 100, CollisionData = 0, Flammability = 125, CorrosionResist = 60 }, "Content/Samples/wood.png");
            Materials.Add(new Material() { Hardness = 100, Process = (byte)0, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/moon_rock.png");
            Materials.Add(new Material() { Hardness = 20, Process = (byte)0, Density = 100, CollisionData = 255, Flammability = 0, CorrosionResist = 10000 }, "Content/Samples/salt.png");
            Materials.Add(new Material() { Hardness = 100000, Process = (byte)0, Density = 50, CollisionData = 0, CorrosionResist = 10000 }, "Content/Samples/brine.png");
            Materials.Add(new Material() { Hardness = 100000, Process = (byte)0, Density = 0, CollisionData = 0, CorrosionResist = 10000}, "Content/Samples/lava.png");
            Materials.Add(new Material() { Hardness = 50, Process = (byte)0, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/silver.png");
            Materials.Add(new Material() { Hardness = 10, Process = (byte)0, Density = 50, CollisionData = 255, CorrosionResist = 10000, Flammability = 1 }, "Content/Samples/coal.png");
            Materials.Add(new Material() { Hardness = 50, Process = (byte)0, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/cobble.png");
            Materials.Add(new Material() { DispersionRate = 2, Hardness = 0, Process = (byte)0, Density = 5, CollisionData = 0, Life = 1000, CorrosionResist = 1000 }, "Content/Samples/smoke.png");
            Materials.Add(new Material() { DispersionRate = 2, Hardness = 0, Process = (byte)0, Density = 100000, CollisionData = 0, CorrosionResist = 10000, StaticHeat = 25000 }, "Content/Samples/plasma.png");

            Language.AddLocalization("material.mat_0.name", "Air");
            for (int i = 0; i < Materials.materials.Count; i++)
            {
                //MaterialType
                Language.AddLocalization($"material.mat_{i + 1}.name", ((MaterialType)((short)i + 1)).ToString().Replace("_", " "));
            }

        }

        public override void MouseWheel()
        {
            
        }

        public override void UnloadContent()
        {
            Materials.DeleteAll();
        }

        public override void LoadContent()
        {
            ChunkCache.cache = Path.Combine(Directory.GetCurrentDirectory(), "Saves", "Save0", "Chunks");
            if (!Directory.Exists(ChunkCache.cache))
                Directory.CreateDirectory(ChunkCache.cache);

            cellworld = new CellWorld(12, 8, rasterizer);

            Sampler.GenTextures();

            post = resources.LoadShader(@"Content\Shaders\postprocess.glsl");

            world.SetDebugDrawer(b2dd);

            cellworld.simulate = false;
            CellWorld.ShowChunks = true;

            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
        }
        public override void DrawFBO(FrameBuffer fbo, Camera camera, int Width, int Height)
        {


            /*Graphics.UseShader(post);

            fbo.tex.Bind(TextureUnit.Texture0);
            CellWorld.lights.glTexture.Bind(TextureUnit.Texture1);

            post.SetInt("world", 0);
            post.SetInt("light", 1);
            post.SetColor3("ambient_light", ambient_light);

            Graphics.Begin(ResourceFactory.Quad, camera);

            Graphics.Draw(new Transform(0, new Vector2(0, 0), new Vector2(Width, Height)));

            Graphics.End();*/
        }
        public override void Draw()
        {

            ParticleEngine.NewFrame(cellworld);

 
            cellworld.front_buffer.UpdateTexture();
            CellWorld.lights.UpdateTexture();


            /*Graphics.UseTextureShader();
            Graphics.Begin(ResourceFactory.Quad, Window.WindowCamera);
            Graphics.Draw(new Transform(0, new Vector2(0, 0), new Vector2(Program.window.Width, -Program.window.Height)), cellworld.front_buffer.glTexture);
            Graphics.End();*/


            world.DebugDraw();
        }


        public void EditorHandleInputInWorld()
        {

        }

        public override void Update()
        {

            if (!ImGui.GetIO().WantCaptureMouse && !ImGui.GetIO().WantCaptureKeyboard)
                EditorHandleInputInWorld();



            if (Mouse.LeftDown)
            {
                Sampler.GetSampler((short)MaterialType.Rock).Create(cellworld, (int)cellworld.MousePosition.X, (int)cellworld.MousePosition.Y, 25, 25);
            }
            try
            {
                Vector2 mouse = Mouse.CursorPos;

                Mouse.CursorPos -= world_editor_pos;
                Mouse.CursorPos *= world_editor_scale;
                cellworld.Step<ChunkWorker>();
                Mouse.CursorPos = mouse;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message, "CELLSTEP");
            }

           

            EventManager.FlushQueue();
            EventManager.ResetTriggers();
        }

    }
}