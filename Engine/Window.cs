using FSEngine.OpenGL;
using ImGuiNET;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = System.Numerics.Vector2;
using TKVector2 = OpenTK.Vector2;
using System.Drawing;
using System.Threading;
using System.IO;
using FSEngine.CellSystem;
using Graphics = FSEngine.OpenGL.Graphics;
using FSEngine.Audio;
using FSEngine.GFX;

namespace FSEngine
{
    public class Window : GameWindow
    {

        public volatile bool run = true;
        public static int sessionID = 1;
        public static Game game;
        public static bool DebugMode = true;
        public ImGuiController controller;
        public static System.Drawing.Color sky = System.Drawing.Color.CornflowerBlue;
        public static int width = 0;
        public static int height = 0;
        bool initialized = false;
        public Window(Type game_type) : base(720, 480, GraphicsMode.Default, "FSEngine")
        {
            game = (Game)Activator.CreateInstance(game_type);

            game.PreloadContent();



            width = 12 * CellWorld.chunk_s;
            height = 8 * CellWorld.chunk_s;

            Debug.Log($"Set Window size to {Size}", "window");

            for (int i = 0; i != Sampler.mappings.Count; i++)
            {
                Sampler.mappings.Values.ElementAt(i).Consume = !DebugMode;
            }

        }


        public Vector2 PointToClient(Vector2 pos)
        {
            Point p = PointToClient(new Point((int)pos.X,(int)pos.Y));
            return new Vector2(p.X, p.Y);
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Mouse.Delta = e.Delta;
            game.MouseWheel();
        }
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            Mouse.CursorPos = new Vector2(e.Mouse.X, e.Mouse.Y);
        }
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            game.OnKey(Keyboard.state, ButtonState.Pressed);
        }
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            game.OnKey(Keyboard.state, ButtonState.Released);
        }
        static FrameBuffer fbo;
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            controller.PressChar(e.KeyChar);

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            controller = new ImGuiController(Width, Height);
            initialized = true;
            GL.Viewport(0, 0, Width, Height);

            game.resources.LoadPrimitives();
            Graphics.TextureShader = game.resources.CompileShader(Shader.fragmentShader);
            Graphics.ParallaxShader = game.resources.CompileShader(Shader.ParallaxShader);

            Graphics.Camera = new Camera(new Vector2(0,0), 1, new Vector2(Width, Height));
            game.LoadContent();
            fbo = new FrameBuffer(width, height);
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            run = false;
            controller.Dispose();

            game.resources.Unload();

            game.UnloadContent();
            fbo.Dispose();
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!initialized)
                return;
            GL.Viewport(0, 0, Width, Height);
            WindowCamera.size = new Vector2(Width, Height);
            // Tell ImGui of the new size
            controller.WindowResized(Width, Height);
            //fbo.Resize(Width, Height);
            game.cellworld.CalculateScale();
        }


        public static void ChangeResolution(int width, int height)
        {
            WorldCamera.size = new Vector2(width, height);
            fbo.Resize(width, height);
            Console.WriteLine("Changed Resolution to {0}x{1}",width,height);
        }

        /// <summary>
        /// Relative to the World
        /// </summary>
        public static Camera WorldCamera = new Camera(new Vector2(0, 0), 1, new Vector2(640, 480));
        /// <summary>
        /// Relative to the Window
        /// </summary>
        public static Camera WindowCamera = new Camera(new Vector2(0, 0), 1, new Vector2(640, 480));
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Keyboard.state = OpenTK.Input.Keyboard.GetState();
            MouseState ms = OpenTK.Input.Mouse.GetState();

            Mouse.LastRight = Mouse.RightDown;
            Mouse.LastLeft = Mouse.LeftDown;

            Mouse.LeftDown = ms.LeftButton == ButtonState.Pressed;
            Mouse.RightDown = ms.RightButton == ButtonState.Pressed;

            Mouse.LeftUp = Mouse.LastLeft && !Mouse.LeftDown;
            Mouse.RightUp = Mouse.LastRight && !Mouse.RightDown;

            Mouse.Middle = ms.MiddleButton == ButtonState.Pressed;
            //md = Mouse.Left || Mouse.Right;

            Time.Tick();
            game.Update();

            game.stats.b2dJoints = game.world.JointCount;
            game.stats.b2dBodies = game.world.BodyCount;
            game.stats.b2dTriangles = Tiles.Tile.triangles;
        }
        public static Rectangle RectFixer(Rectangle rect)
        {
            if (rect.Width < 0)
            {
                rect.X = rect.X + rect.Width;
                rect.Width = Math.Abs(rect.Width);
            }
            if (rect.Height < 0)
            {
                rect.Y = rect.Y + rect.Height;
                rect.Height = Math.Abs(rect.Height);
            }
            return rect;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
   
            if (!initialized)
                return;


            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.ClearColor(sky.R / 255f, sky.G / 255f, sky.B / 255f, sky.A / 255f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            
            fbo.BeginDrawing();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.ClearColor(sky.R / 255f, sky.G / 255f, sky.B / 255f, sky.A / 255f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


                game.Draw();


                fbo.EndDrawing();

                game.DrawFBO(fbo, WindowCamera, Width, Height);

                controller.Update(this, (float)e.Time);

                game.DrawUI();


                controller.Render(); // Render UI


            ImGuiController.CheckGLError("End of frame");
            SwapBuffers();
        }

    }
}
