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

namespace FSEngine.Tests
{
    public enum MouseAction
    {
        Grab,
        Fill,
        Earse
    }
    public class TestGame : Game
    {

        public Shader post;

        public Texture pickaxe;
        public Texture sword;

        public Container inventory = new Container(10, 5);
        
        public Tile tile_a, tile_b, tile_c;
        public Body GroundBody;


        public short selected = 1;
        public int brush = 15;
        public int tfps = 60;


        public bool rb2d = true;
        public bool b2daabb = false;
        public bool b2dtrans = true;
        public bool b2dshapes = false;
        public bool b2djoints = true;

        public MouseAction action;
        public TestGame() : base()
        {

        }

        public void EnumCombo<T>(string label, ref T selected)
        {

            if (ImGui.BeginCombo(label, selected.ToString()))
            {
                foreach (string s in Enum.GetNames(typeof(T)))
                {
                    if (ImGui.Selectable(s))
                        selected = (T)Enum.Parse(typeof(T), s);
                }
                ImGui.EndCombo();
            }
        }

        public override void DrawUI()
        {
            #region Overlay
            ImOverlay.Begin(0f);
            ImGui.Text($"FPS: {cellworld.frame/(Time.sw.Elapsed.Seconds+1)}");
            ImGui.Text($"Updated Cells: {CellWorld.CellsUpdated}");
            ImGui.Text($"Position: {cellworld.PixelOffset}");
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
            ImGui.SetNextItemWidth(128);
            ImOverlay.SliderInt("Brush", ref brush, 1, 100);
            ImGui.SetNextItemWidth(128);
            float grav = world.Gravity.Y;
            ImOverlay.SliderFloat("Gravity", ref grav, 1, 100);
            world.Gravity = new Vector2(0, grav);

            ImGui.SetNextItemWidth(128);
            ImOverlay.SliderInt("SPS", ref tfps, 0, 60);
            cellworld.simulate = tfps != 0;

            ImOverlay.MakeInteractable();
            ImGui.SetNextItemWidth(128);
            EnumCombo("Action", ref action);

            ImOverlay.MakeInteractable();
            if (Sampler.GetSampler(selected).sampler != null)
            {
                ImGui.Image((IntPtr)Sampler.GetSampler(selected).sampler.glTexture.id, new Vector2(24, 24));
                ImGui.SameLine();
            }


            ImGui.Text(Language.Localize($"material.mat_{selected}.name"));

            ImOverlay.End();
            #endregion

            if(!ImGui.GetIO().WantCaptureMouse && mj != null)
            {
                ImGui.BeginTooltip();
                Tile t = Tile.LoopUp[(int)mj.BodyB.UserData];
                ImGui.Text($"Tile {t.id}");
                ImGui.Text($"Size {t.width}, {t.height}");
                ImGui.Text($"Position {t.x}, {t.y}");
                ImGui.Text($"Rotation {(int)Math.Abs(t.rotation / (Math.PI/ 180)) % 360}");
                ImGui.EndTooltip();
            }

            //InventoryManager.Draw();
            DrawStats();
        }



  
        public override void PreloadContent()
        {
            Materials.Add(new Material() { Hardness = 20, Process = (byte)Processor.Sand, Density = 80, CollisionData = 255, Flammability = 0, CorrosionResist = 15 }, "Content/Samples/sand.png");
            Materials.Add(new Material() { Hardness = 100, Process = (byte)Processor.None, Density = 100, CollisionData = 255, CanMelt = true, Flammability = 0, CorrosionResist = 5 }, "Content/Samples/iron.png");
            Materials.Add(new Material() { DispersionRate = 5, Hardness = 0, Process = (byte)Processor.Liquid, Density = 25, CollisionData = 0, CorrosionResist = 1000 }, "Content/Samples/water.png");
            Materials.Add(new Material() { DispersionRate = 2, Hardness = 0, Process = (byte)Processor.Acid, Density = 15, CollisionData = 0, Corrosive = 10, CorrosionResist = 1000 }, "Content/Samples/acid.png");
            Materials.Add(new Material() { DispersionRate = 2, Hardness = 0, Process = (byte)Processor.Gas, Density = 5, CollisionData = 0, Life = 1000, CorrosionResist = 1000 }, "Content/Samples/toxic_gas.png");
            Materials.Add(new Material() { Hardness = 0, Process = (byte)Processor.Liquid, Density = 50, CollisionData = 255, CorrosionResist = 1000 }, "Content/Samples/lava.png");
            Materials.Add(new Material() { Hardness = 75, Process = (byte)Processor.None, Density = 100, CollisionData = 255, CorrosionResist = 50, Flammability = 0, CanMelt = true }, "Content/Samples/glass.png");
            Materials.Add(new Material() { Hardness = 5, Process = (byte)Processor.None, Density = 75, CollisionData = 255, CorrosionResist = 1000 }, "Content/Samples/ice.png");
            Materials.Add(new Material() { DispersionRate = 3, Hardness = 0, Process = (byte)Processor.Liquid, Density = 10, CollisionData = 255, CorrosionResist = 1000 }, "Content/Samples/liquid_air.png");
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
            Materials.Add(new Material() { Hardness = 100000, Process = (byte)Processor.Ember, Density = 0, CollisionData = 0, CorrosionResist = 10000 }, "Content/Samples/lava.png");
            Materials.Add(new Material() { Hardness = 50, Process = (byte)Processor.None, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/silver.png");
            Materials.Add(new Material() { Hardness = 10, Process = (byte)Processor.Sand, Density = 50, CollisionData = 255, CorrosionResist = 10000, Flammability = 1 }, "Content/Samples/coal.png");
            Materials.Add(new Material() { Hardness = 50, Process = (byte)Processor.None, Density = 50, CollisionData = 255, CorrosionResist = 10000 }, "Content/Samples/cobble.png");

            Language.AddLocalization("material.mat_0.name", "Air");
            Language.AddLocalization("material.mat_1.name", "Sand");
            Language.AddLocalization("material.mat_2.name", "Iron");
            Language.AddLocalization("material.mat_3.name", "Water");
            Language.AddLocalization("material.mat_4.name", "Acid");
            Language.AddLocalization("material.mat_5.name", "Gas");
            Language.AddLocalization("material.mat_6.name", "Lava");

            InventoryManager.Add(inventory);
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
        B2DDebug b2dd = new B2DDebug();

        public override void LoadContent()
        {
            ChunkCache.cache = Path.Combine(Directory.GetCurrentDirectory(), "Saves", "Save0", "Chunks");
            if (!Directory.Exists(ChunkCache.cache))
                Directory.CreateDirectory(ChunkCache.cache);

            cellworld = new CellWorld(12, 8, new ChunkManager(), new TestRasterizer());

            Sampler.GenTextures();

            post = resources.LoadShader(@"Content\Shaders\postprocess.glsl");
            pickaxe = resources.LoadTexture(@"Content\Sprites\Pickaxe.png");
            sword = resources.LoadTexture(@"Content\Sprites\Sword.png");

            Item pick = new Item() { Name = "Pickaxe", count = 1, stackable = false, Description = "A Pickaxe", id = 0, texture = (IntPtr)pickaxe.id };
            Item swrd = new Item() { Name = "Sword", count = 1, stackable = false, Description = "A Sword", id = 1, texture = (IntPtr)sword.id };

            inventory.slots[0] = new Slot(pick);
            inventory.slots[4] = new Slot(swrd);

            Mesher mesher = new Mesher(new Bitmap("test.png"));
            Vector2[][] verts = mesher.Mesh();

            tile_a = Tile.Create(world, BodyType.DynamicBody, 720, 360, "test.png", verts);
            tile_b = Tile.Create(world, BodyType.DynamicBody, 708, 510 , 10, 10);
            tile_c = Tile.Create(world, BodyType.StaticBody, 720, 600, 720, 10);

            GroundBody = world.CreateBody(new BodyDef());

            world.SetDebugDrawer(b2dd);
        }

        public override void Draw()
        {

            ParticleEngine.NewFrame(cellworld);

            cellworld.front_buffer.UpdateTexture();
            CellWorld.lights.UpdateTexture();

            post.Bind();

            cellworld.front_buffer.glTexture.Bind(TextureUnit.Texture0);
            CellWorld.lights.glTexture.Bind(TextureUnit.Texture1);

            ResourceFactory.Quad.Bind();

            post.SetMatrix4x4("model", new Transform(0, new Vector2(0, 0), new Vector2(Program.window.Width, Program.window.Height)).GetModelMatrix());
            post.SetMatrix4x4("projection", Window.WindowCamera.GetProjectionMatrix());
            post.SetInt("world", 0);
            post.SetInt("light", 1);

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
            }

        }

        MouseQuery query = new MouseQuery();
        MouseJoint mj;
        
        public override void Update()
        {
            world.Step(1 / 60f, 8, 3);
            
            tile_a.Tick(cellworld);
            tile_b.Tick(cellworld);
            tile_c.Tick(cellworld);

            Vector2 lp = cellworld.PixelOffset;
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
                if (cellworld.PixelOffset.X < 100)
                {
                    cellworld.SetPixelOffset(new Vector2(100, cellworld.PixelOffset.Y));
                }
                if (cellworld.PixelOffset.Y < 100)
                {
                    cellworld.SetPixelOffset(new Vector2(cellworld.PixelOffset.X, 100));
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
                                    MaxForce = 1000.0f * body.Mass
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