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
using FSEngine;
using FSEngine.Tiles;
using Box2DSharp.Dynamics;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics.Joints;
using FSEngine.Geometry;
using System.Collections.Generic;
using FSEngine.IO;

namespace FSEngine.Tests
{
    public enum MouseAction
    {
        Grab,
        Fill,
        Earse
    }
    public enum VisionType
    {
        Normal,
        Thermal
    }

    public class TestEntity : Entity
    {
        int i = 0;
        public TestEntity()
        {
            x = 500;
            y = 500;
        }
		public override void Save(BinaryWriter writer)
		{
            Console.WriteLine("Save ME");
        }
		public override void Update()
		{
            i++;
            if(i % 100 == 0)
            Console.WriteLine("[{0}] Tick", i);
        }
	}
    public class TestGame : Game
    {

        public Shader post;
        public Body GroundBody;
        public B2DDebug b2dd = new B2DDebug();
        public MouseQuery query = new MouseQuery();
        public MouseJoint mj;

        public short selected = (short)MaterialType.Lava;
        public int brush = 15;
        public int tfps = 60;


        public bool rb2d = false;
        public bool b2daabb = false;
        public bool b2dtrans = false;
        public bool b2dshapes = true;
        public bool b2djoints = true;

        public MouseAction action = MouseAction.Fill;

        public VisionType vision = VisionType.Normal;

        GFX.Color ambient_light = GFX.Color.FromArgb(160,160,160);

        ThermalVision thermal_rasterizer = new ThermalVision();
        NormalVision normal_rasterizer = new NormalVision();
        Parallax background;

        Container container = new Container(4,4);

        public TestGame() : base()
        {

        }

        bool world_gen = true;

        static MaterialType from = MaterialType.Grass;
        static MaterialType to = MaterialType.Fire;
        public void Swap()
        {
            Sampler from_sampler = Sampler.mappings[(short)from];
            Sampler to_sampler = Sampler.mappings[(short)to];

            TSBitmap fs = from_sampler.sampler;
            uint fw = from_sampler.w, fh = from_sampler.h;
            from_sampler.sampler = to_sampler.sampler;
            from_sampler.w = to_sampler.w;
            from_sampler.h = to_sampler.h;
            to_sampler.w = fw;
            to_sampler.h = fh;
            to_sampler.sampler = fs;

            Material from_material = Materials.materials[(short)from - 1];
            Material to_material = Materials.materials[(short)to - 1];

            Materials.materials[(short)from - 1] = to_material;
            Materials.materials[(short)to - 1] = from_material;

            Language.localized[$"material.mat_{(short)from}.name"] = to.ToString().Replace("_", " ") + " Swapped";
            Language.localized[$"material.mat_{(short)to}.name"] = from.ToString().Replace("_", " ") + " Swapped";

            cellworld.FroceChunkUpdates();
        }
        WorldGenerator generator = new WorldGenerator();
        public override void DrawUI()
        {
            InventoryManager.Draw();
            DrawStats();
            #region Overlay
            ImOverlay.Begin(0f, new Vector2(1f, 1f));
            //ImGui.Text($"FPS: {cellworld.frame/(Time.Elapsed.Seconds+1)}");
            ImGui.Text($"FPS: {Time.fps}");
            ImGui.Text($"Updated Cells: {CellWorld.CellsUpdated}");
            ImGui.Text($"Position: {cellworld.PixelOffset}");

  
            if(ImOverlay.CollapsingHeader("Debug Rendering", new Vector2(128, 18)))
            {

                ImOverlay.Checkbox("Show Chunks", ref CellWorld.ShowChunks);
                if (CellWorld.ShowChunks)
                {
                    ImGui.Text("  ");
                    ImGui.SameLine();
                    ImOverlay.Checkbox("Show Dirty Rects", ref CellWorld.ShowDirtyRect);
                }
                ImOverlay.Checkbox("Show Box2D", ref rb2d);
                if (rb2d)
                {
                    ImGui.Text("  ");
                    ImGui.SameLine();
                    ImOverlay.Checkbox("Show AABB", ref b2daabb);
                    ImGui.Text("  ");
                    ImGui.SameLine();
                    ImOverlay.Checkbox("Show Transform", ref b2dtrans);
                    ImGui.Text("  ");
                    ImGui.SameLine();
                    ImOverlay.Checkbox("Show Shapes", ref b2dshapes);
                    if (b2dshapes)
                    {
                        ImGui.Text("    ");
                        ImGui.SameLine();
                        ImOverlay.Checkbox("As Outline", ref b2dd.DrawShapeAsOutline);
                    }
                    ImGui.Text("  ");
                    ImGui.SameLine();
                    ImOverlay.Checkbox("Show Joints", ref b2djoints);

                }
                b2dd.Flags = 0;
                if (b2daabb)
                    b2dd.Flags |= Box2DSharp.Common.DrawFlag.DrawAABB;
                if (b2dtrans)
                    b2dd.Flags |= Box2DSharp.Common.DrawFlag.DrawCenterOfMass;
                if (b2dshapes)
                    b2dd.Flags |= Box2DSharp.Common.DrawFlag.DrawShape;
                if (b2djoints)
                    b2dd.Flags |= Box2DSharp.Common.DrawFlag.DrawJoint;
            }

            if (ImOverlay.CollapsingHeader("Physics", new Vector2(128, 18)))
            {
                ImGui.SetNextItemWidth(128);
                ImOverlay.SliderInt("Edit Tolerance", ref Tile.EditTolerance, 1, 20);
                ImGui.SetNextItemWidth(128);
                ImOverlay.SliderInt("Tile Tick Rate", ref Tile.TickRate, 1, 20);
                int res = (int)(1f / Tile.MeshResolution);
                ImGui.SetNextItemWidth(128);
                ImOverlay.SliderInt("Mesh Resolution", ref res, 10, 100);

                Tile.MeshResolution = 1f / res;
                float grav = world.Gravity.Y;
                ImGui.SetNextItemWidth(128);
                ImOverlay.SliderFloat("Gravity", ref grav, 1, 100);
                world.Gravity = new Vector2(0, grav);
            }

            if (ImOverlay.CollapsingHeader("Edit", new Vector2(128, 18)))
            {
                ImGui.SetNextItemWidth(128);
                ImOverlay.SliderInt("Brush", ref brush, 1, 100);


                ImGui.SetNextItemWidth(128);
                ImOverlay.MakeInteractable();
                Editor.Widgets.EnumCombo("Action", ref action);

                ImOverlay.MakeInteractable();
                if (Sampler.GetSampler(selected).sampler != null)
                {
                    ImGui.Image((IntPtr)Sampler.GetSampler(selected).sampler.glTexture.id, new Vector2(24, 24));
                    ImGui.SameLine();
                }


                ImGui.Text(Language.Localize($"material.mat_{selected}.name"));

            }

           ImOverlay.Checkbox("Render World", ref CellWorld.render);
                if (world_gen && ImOverlay.Button("Generate World"))
            {
                world_gen = false;

                Debug.Log("Generating World");
                generator.SpawnStructs();
                for (int x = 0; x < 75; x++)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        CellChunk chunk = generator.Gen(x, y + 9);
                        if(chunk.filledcells > 0)
                            cellworld.Push(chunk);
                    }
                }
                Debug.Log("World Generated");
            }

            ImGui.SetNextItemWidth(128);
            VisionType before = vision;
            Editor.Widgets.EnumCombo("Vision", ref vision);
            ImOverlay.MakeInteractable();
            if (vision != before)
            {
                if(vision == VisionType.Thermal)
                    cellworld.SetRasterizer(thermal_rasterizer);
                else if(vision == VisionType.Normal)
                    cellworld.SetRasterizer(normal_rasterizer);
            }

            int r = ambient_light.R;
            ImGui.SetNextItemWidth(128);
            ImOverlay.SliderInt("Light R", ref r, 0, 255);
            ambient_light.R = (byte)r; 
            int g = ambient_light.G;
            ImGui.SetNextItemWidth(128);
            ImOverlay.SliderInt("Light G", ref g, 0, 255);
            ambient_light.G = (byte)g; 
            int b = ambient_light.B;
            ImGui.SetNextItemWidth(128);
            ImOverlay.SliderInt("Light B", ref b, 0, 255);
            ambient_light.B = (byte)b;
            ImOverlay.End();
            #endregion

            if(!ImGui.GetIO().WantCaptureMouse)
            {
                if(mj != null)
                {
                    ImGui.BeginTooltip();
                    Tile t = Tile.LoopUp[(int)mj.BodyB.UserData];
                    ImGui.Text($"Tile {t.id}");
                    ImGui.Text($"Size {t.width}, {t.height}");
                    ImGui.Text($"Position {t.x}, {t.y}");
                    ImGui.Text($"Rotation {(int)Math.Abs(t.rotation / (Math.PI / 180)) % 360}");
                    ImGui.EndTooltip();
                }
                else
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"{(int)cellworld.MousePosition.X}, {(int)cellworld.MousePosition.Y}");
                    CellChunk c = cellworld.GetChunk((int)cellworld.MousePosition.X, (int)cellworld.MousePosition.Y);
                    ImGui.Text(Language.Localize($"material.mat_{c.cells[(int)cellworld.MousePosition.X % 60, (int)cellworld.MousePosition.Y % 60].type}.name"));
                    ImGui.Text($"heat: {c.cells[(int)cellworld.MousePosition.X % 60, (int)cellworld.MousePosition.Y % 60].heat}");
                    ImGui.Text($"updated cells: {c.updatedcells}");
                    ImGui.EndTooltip();
                }

            }

        }


        public override void PreloadContent()
        {
            Materials.Add(new Material() { Hardness = 20, Process = (byte)Processor.Sand, Density = 80, CollisionData = 255, Flammability = 0, CorrosionResist = 15 }, "Content/Samples/sand.png");
            Materials.Add(new Material() { Hardness = 100, Process = (byte)Processor.None, Density = 100, CollisionData = 255, CanMelt = true, Flammability = 0, CorrosionResist = 5 }, "Content/Samples/iron.png");
            Materials.Add(new Material() { DispersionRate = 5, Hardness = 0, Process = (byte)Processor.Liquid, Density = 25, CollisionData = 0, CorrosionResist = 1000 }, "Content/Samples/water.png");
            Materials.Add(new Material() { DispersionRate = 2, Hardness = 0, Process = (byte)Processor.Acid, Density = 15, CollisionData = 0, Corrosive = 10, CorrosionResist = 1000 }, "Content/Samples/acid.png");
            Materials.Add(new Material() { DispersionRate = 2, Hardness = 0, Process = (byte)Processor.Gas, Density = 5, CollisionData = 0, Life = 1000, CorrosionResist = 1000 }, "Content/Samples/toxic_gas.png");
            Materials.Add(new Material() { Hardness = 0, Process = (byte)Processor.Liquid, Density = 50, CollisionData = 255, CorrosionResist = 1000, StaticHeat = 1800 }, "Content/Samples/lava.png");
            Materials.Add(new Material() { Hardness = 75, Process = (byte)Processor.None, Density = 100, CollisionData = 255, CorrosionResist = 50, Flammability = 0, CanMelt = true }, "Content/Samples/glass.png");
            Materials.Add(new Material() { Hardness = 5, Process = (byte)Processor.None, Density = 75, CollisionData = 255, CorrosionResist = 1000 }, "Content/Samples/ice.png");
            Materials.Add(new Material() { DispersionRate = 3, Hardness = 0, Process = (byte)Processor.Liquid, Density = 10, CollisionData = 255, CorrosionResist = 1000, StaticHeat = -16000 }, "Content/Samples/liquid_air.png");
            Materials.Add(new Material() { DispersionRate = 3, Hardness = 0, Process = (byte)Processor.Liquid, Density = 70, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/suspicious_liquid.png");
            Materials.Add(new Material() { Hardness = 20, Process = (byte)Processor.None, Density = 100, CollisionData = 0, Flammability = 95, CorrosionResist = 60 }, "Content/Samples/wood.png");
            Materials.Add(new Material() { Hardness = 1000, Process = (byte)Processor.None, Density = 1000, CollisionData = 255, CorrosionResist = 9 }, "Content/Samples/exotic_matter.png");// up
            Materials.Add(new Material() { Hardness = 2, Process = (byte)Processor.Sand, Density = 100, CollisionData = 255, Flammability = 0, CorrosionResist = 10000 }, "Content/Samples/rust.png");
            Materials.Add(new Material() { Hardness = 10, Process = (byte)Processor.None, Density = 200, CollisionData = 255, CanMelt = true, Flammability = 0, CorrosionResist = 10000 }, "Content/Samples/gold.png");
            Materials.Add(new Material() { Hardness = 1000, Process = (byte)Processor.None, Density = 10000, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/m_stone.png");
            Materials.Add(new Material() { Hardness = 50, Process = (byte)Processor.None, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/rock.png");
            Materials.Add(new Material() { Hardness = 0, Process = (byte)Processor.None, Density = 10, CollisionData = 0, Flammability = 50, CorrosionResist = 10000 }, "Content/Samples/grass.png");
            Materials.Add(new Material() { Hardness = 0, Process = (byte)Processor.None, Density = 25, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/dirt.png");
            Materials.Add(new Material() { Hardness = 100000, Process = (byte)Processor.Fire, Density = 0, Life = 250, CollisionData = 0, CorrosionResist = 10000 }, "Content/Samples/lava.png");
            Materials.Add(new Material() { Hardness = 80, Process = (byte)Processor.None, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/sandstone.png");
            Materials.Add(new Material() { Hardness = 20, Process = (byte)Processor.None, Density = 100, CollisionData = 0, Flammability = 125, CorrosionResist = 60 }, "Content/Samples/wood.png");
            Materials.Add(new Material() { Hardness = 100, Process = (byte)Processor.None, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/moon_rock.png");
            Materials.Add(new Material() { Hardness = 20, Process = (byte)Processor.Salt, Density = 100, CollisionData = 255, Flammability = 0, CorrosionResist = 10000 }, "Content/Samples/salt.png");
            Materials.Add(new Material() { Hardness = 100000, Process = (byte)Processor.Liquid, Density = 50, CollisionData = 0, CorrosionResist = 10000 }, "Content/Samples/brine.png");
            Materials.Add(new Material() { Hardness = 100000, Process = (byte)Processor.Ember, Density = 0, CollisionData = 0, CorrosionResist = 10000}, "Content/Samples/lava.png");
            Materials.Add(new Material() { Hardness = 50, Process = (byte)Processor.None, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/silver.png");
            Materials.Add(new Material() { Hardness = 10, Process = (byte)Processor.Sand, Density = 50, CollisionData = 255, CorrosionResist = 10000, Flammability = 1 }, "Content/Samples/coal.png");
            Materials.Add(new Material() { Hardness = 50, Process = (byte)Processor.None, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/cobble.png");
            Materials.Add(new Material() { DispersionRate = 2, Hardness = 0, Process = (byte)Processor.Gas, Density = 5, CollisionData = 0, Life = 1000, CorrosionResist = 1000 }, "Content/Samples/smoke.png");
            Materials.Add(new Material() { DispersionRate = 2, Hardness = 0, Process = (byte)Processor.Gas, Density = 100000, CollisionData = 0, CorrosionResist = 10000, StaticHeat = 25000 }, "Content/Samples/plasma.png");

            Language.AddLocalization("material.mat_0.name", "Air");/*
            Language.AddLocalization("material.mat_1.name", "Sand");
            Language.AddLocalization("material.mat_2.name", "Iron");
            Language.AddLocalization("material.mat_3.name", "Water");
            Language.AddLocalization("material.mat_4.name", "Acid");
            Language.AddLocalization("material.mat_5.name", "Gas");
            Language.AddLocalization("material.mat_6.name", "Lava");*/
            for (int i = 0; i < Materials.materials.Count; i++)
            {
                //MaterialType
                Language.AddLocalization($"material.mat_{i + 1}.name", ((MaterialType)((short)i + 1)).ToString().Replace("_", " "));
            }

        }

        public override void MouseWheel()
        {
            float angle = 0;
            if (FSEngine.Mouse.Delta > 0)
            {                
                selected++;
               
            }
            else
            {
                selected--;

            }
                
            if (selected > Materials.materials.Count)
                selected = 0;
            if (selected < 0)
                selected = (short)Materials.materials.Count;
        }

        public override void UnloadContent()
        {
            Materials.DeleteAll();
        }

        public override void LoadContent()
        {
            Item i = new Item();
            i.Name = "test";
            i.texture = (IntPtr)resources.LoadTexture("Content\\Sprites\\Sword.png").id;
            i.count = 1;
            i.id = 1;
            i.Description = "Test item";

            container.slots[0] = new Slot(i);
            container.slots[1] = new Slot(i.Clone());
            container.slots[1].item.Name = "test 2";
            ChunkCache.cache = Path.Combine(Directory.GetCurrentDirectory(), "Saves", "Save0", "Chunks");
            if (!Directory.Exists(ChunkCache.cache))
                Directory.CreateDirectory(ChunkCache.cache);

            cellworld = new CellWorld(12, 8, normal_rasterizer);

            Sampler.GenTextures();

            post = resources.LoadShader(@"Content\Shaders\postprocess.glsl");

            GroundBody = world.CreateBody(new BodyDef());

            world.SetDebugDrawer(b2dd);

            background = XMLLoader.LoadParallax(this, "Content/Sprites/Sky/parallax.xml");

            Entities.Register(new TestEntity());
        }
        public override void DrawFBO(FrameBuffer fbo, Camera camera, int Width, int Height)
        {


            Graphics.UseShader(post);

            fbo.tex.Bind(TextureUnit.Texture0);
            CellWorld.lights.glTexture.Bind(TextureUnit.Texture1);

            post.SetInt("world", 0);
            post.SetInt("light", 1);
            post.SetColor3("ambient_light", ambient_light);

            Graphics.Begin(ResourceFactory.Quad, camera);

            Graphics.Draw(new Transform(0, new Vector2(0, 0), new Vector2(Width, Height)));

            Graphics.End();
        }
        public override void Draw()
        {
           
            ParticleEngine.NewFrame(cellworld);

 
            cellworld.front_buffer.UpdateTexture();
            CellWorld.lights.UpdateTexture();


            Graphics.UseTextureShader();
            Graphics.Begin(ResourceFactory.Quad, Window.WindowCamera);
            Graphics.DrawParallax(new Transform(0, new Vector2(0, 0), new Vector2(Program.window.Width, -Program.window.Height)), background);
            Graphics.Draw(new Transform(0, new Vector2(0, 0), new Vector2(Program.window.Width, -Program.window.Height)), cellworld.front_buffer.glTexture);
            Graphics.End();

            /*   
            post.Bind();

            cellworld.front_buffer.glTexture.Bind(TextureUnit.Texture0);
            CellWorld.lights.glTexture.Bind(TextureUnit.Texture1);

            ResourceFactory.Quad.Bind();

            post.SetMatrix4x4("model", new Transform(0, new Vector2(0, 0), new Vector2(Program.window.Width, Program.window.Height)).GetModelMatrix());
            post.SetMatrix4x4("projection", Window.WindowCamera.GetProjectionMatrix());
            post.SetInt("world", 0);
            post.SetInt("light", 1);
            post.SetColor3("ambient_light", ambient_light);
            ResourceFactory.Quad.Draw();

            ResourceFactory.Quad.Unbind();


            cellworld.front_buffer.glTexture.Unbind(TextureUnit.Texture0);
            CellWorld.lights.glTexture.Unbind(TextureUnit.Texture1);
            post.Unbind();

            if(rb2d)
            {
                b2dd.PushPixelMatrix((int)cellworld.PixelOffset.X, (int)cellworld.PixelOffset.Y);
               
                world.DebugDraw();
               
                GL.PopMatrix();
            }*/

        }


        public override void Update()
        {
            Entities.Tick();
            world.Step(1 / 60f, 8, 3);

            if (cellworld.PixelOffset.X < 100)
            {
                cellworld.SetPixelOffset(new Vector2(100, cellworld.PixelOffset.Y));
            }
            if (cellworld.PixelOffset.Y < 100)
            {
                cellworld.SetPixelOffset(new Vector2(cellworld.PixelOffset.X, 100));
            }

            if (!ImGui.GetIO().WantCaptureMouse)
            {


                if (FSEngine.Keyboard.IsKeyDown(Key.W))
                {
                    cellworld.SetPixelOffset(cellworld.PixelOffset + new Vector2(0, -5));
                }
                if (FSEngine.Keyboard.IsKeyDown(Key.S))
                {
                    cellworld.SetPixelOffset(cellworld.PixelOffset + new Vector2(0, 5));
                }
                if (FSEngine.Keyboard.IsKeyDown(Key.D))
                {
                    cellworld.SetPixelOffset(cellworld.PixelOffset + new Vector2(5, 0));
                }
                if (FSEngine.Keyboard.IsKeyDown(Key.A))
                {
                    cellworld.SetPixelOffset(cellworld.PixelOffset + new Vector2(-5, 0));
                }
                if (FSEngine.Keyboard.IsKeyDown(Key.E))
                {
                    WorldGenerator.geode.Create(cellworld, cellworld.MousePosition);
                }
                if (FSEngine.Keyboard.IsKeyDown(Key.F))
                {
                    WorldGenerator.pond.Create(cellworld, cellworld.MousePosition);
                }

                if (Mouse.LeftDown)
                {
                    if (action == MouseAction.Earse)
                    {
                        Sampler.ClearRect(cellworld, new Rectangle((int)cellworld.MousePosition.X, (int)cellworld.MousePosition.Y, brush, brush));
                    }
                    else if (action == MouseAction.Fill)
                    {
                        Sampler.GetSampler(selected).Create(cellworld, new Rectangle((int)cellworld.MousePosition.X - brush / 2, (int)cellworld.MousePosition.Y - brush / 2, brush, brush), FillMode.Fill);
                    }
                    else if (action == MouseAction.Grab)
                    {

                        if (mj == null)
                        {
                            query.Reset(cellworld.MousePosition);
                            world.QueryAABB(query, new Box2DSharp.Collision.AABB(cellworld.MousePosition - new Vector2(0.001f, 0.001f), cellworld.MousePosition + new Vector2(0.001f, 0.001f)));

                            if (query.found != null)
                            {

                                float frequencyHz = 5.0f;
                                float dampingRatio = 0.7f;

                                Body body = query.found.Body;
                                MouseJointDef jd = new MouseJointDef
                                {
                                    BodyA = GroundBody,
                                    BodyB = body,
                                    Target = cellworld.MousePosition,
                                    MaxForce = 10000.0f * body.Mass
                                };
                                JointUtils.LinearStiffness(out jd.Stiffness, out jd.Damping, frequencyHz, dampingRatio, jd.BodyA, jd.BodyB);
                                mj = (MouseJoint)world.CreateJoint(jd);
                                body.IsAwake = true;
                            }
                        }
                        else
                        {
                            if (mj?.Target != cellworld.MousePosition)
                                mj?.SetTarget(cellworld.MousePosition);
                        }
                    }
                    
                }
                if(Mouse.LeftUp)
                {
                    if(mj != null)
                    {
                        world.DestroyJoint(mj);
                        mj = null;
                    }
                }
            } 
            try
            {

                cellworld.Step<TestWorker>(tfps);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message, "CELLSTEP");
            }

           

            EventManager.FlushQueue();
            EventManager.ResetTriggers();
        }

    }

    public class MouseQuery : IQueryCallback
    {
        public Fixture found = null;
        public Vector2 point;
        public void Reset(Vector2 point)
        {
            found = null;
            this.point = point;
        }
        public bool QueryCallback(Fixture fixture)
        {
            if(fixture.Body.BodyType == BodyType.DynamicBody)
            {
                if(fixture.TestPoint(point))
                {
                    found = fixture;
                    return false;
                }
            }

            return true;
        }
    }
}