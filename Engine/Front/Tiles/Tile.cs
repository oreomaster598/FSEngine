using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using FSEngine.CellSystem;
using FSEngine.CellSystem.Effects.Particles;
using Box2DSharp.Dynamics;
using Box2DSharp.Collision.Shapes;

namespace FSEngine.Tiles
{
    public class Tile
    {
        public Cell[,] cells;
        internal int x = 0, y = 0, nx = 0, ny = 0;
        internal float rotation = 0 * (float)Math.PI / 180f, nr = 0;
        internal int width = 0, height = 0;
        public int id = 1;
        private bool init = true;
        public float midx, midy;
        public Body body;
        public bool asleep;
        bool Poly = false;
        public Tile(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.nx = x;
            this.ny = y;
            this.width = width; 
            this.height = height;
            this.midx = (float)width / 2f;
            this.midy = (float)height / 2f;
            cells = new Cell[width, height];
        }
    
        public Vector2 RotateUV(Vector2 uv, float rotation)
        {
            return new Vector2(
              (float)Math.Round((float)Math.Cos(rotation) * (uv.X - midx) + (float)Math.Sin(rotation) * (uv.Y - midy) + midx),
              (float)Math.Round((float)Math.Cos(rotation) * (uv.Y - midy) - (float)Math.Sin(rotation) * (uv.X - midx) + midy)
            );
        }

        public void CollectCells(CellWorld world, int sx, int sy)
        {

            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                {

                    Vector2 rot = RotateUV(new Vector2(x, y), rotation);

                    int _x = (int)rot.X;
                    int _y = (int)rot.Y;


                    if (_x > -1 && _y > -1 && _x < width && _y < height)
                    {

                        //cells[_x, _y] = world.GetCell(sx + x, sy + y);
                        Cell c = world.GetCell(sx + x, sy + y);

                        if (cells[_x, _y].type > 0)
                        {
                            world.Clear(sx + x, sy + y);
                            cells[_x, _y] = c;
                        }
                    }
                    
                }
        }

        public void InsertArray(CellWorld world, int sx, int sy)
        {
            for (int x = 0; x != width; x++)
                for (int y = 0; y != height; y++)
                {
                    Vector2 rot = RotateUV(new Vector2(x, y), rotation);

                    int _x = (int)rot.X;
                    int _y = (int)rot.Y;

                    if (_x > -1 && _y > -1 && _x < width && _y < height && cells[_x, _y].type > 0)
                    {
                        cells[_x, _y].owner = (short)id;

                        Cell c = world.GetCell(sx + x, sy + y);

                        if(cells[_x, _y].A > 0)
                        {
                            world.SetCell(sx + x, sy + y, cells[_x, _y]);
                            if (c.type > 0 && c.owner == 0)
                            {
                                ParticleEngine.AddParticleWithVelocity(c, sx + x, sy + y);
                            }
                        }

                    }

                }
        }
        static int count = 0;
        internal static int triangles;
        public static Dictionary<int, Tile> LoopUp = new Dictionary<int, Tile>();
        public static Tile Create(World world, BodyType type, float px, float py, int sx = 20, int sy = 20)
        {
            count++;
            Tile t = new Tile(400,400, sx+10,sy+10);
            t.id = count;
            LoopUp.Add(t.id,t);

            var bodyDef = new BodyDef
            {
                BodyType = type,
                AllowSleep = false,
                Position = new Vector2(px, py)
            };
            t.body = world.CreateBody(bodyDef);

            PolygonShape shape = new PolygonShape();
            shape.SetAsBox(sx/2, sy/2);
            t.body.CreateFixture(shape, 5);

            t.body.UserData = t.id;


            Sampler samp = Sampler.GetSampler((short)MaterialType.Iron);
            for (int x = 5; x < t.width-5; x++)
            {
                for (int y = 5; y < t.height-5; y++)
                {
                    t.cells[x, y] = samp.GetCell(x + 1, y + 1);
                }
            }
            Tile.triangles += 2;
            return t;
        }
        public static Tile Create(World world, BodyType type, float px, float py, string image)
        {
            Bitmap bmp = new Bitmap(image);

            int sx = bmp.Width;
            int sy = bmp.Height;

            count++;
            Tile t = new Tile(400, 400, sx + 10, sy + 10);
            t.id = count;
            LoopUp.Add(t.id, t);

            var bodyDef = new BodyDef
            {
                BodyType = type,
                AllowSleep = false,
                Position = new Vector2(px, py)
            };
            t.body = world.CreateBody(bodyDef);

            PolygonShape shape = new PolygonShape();
            shape.SetAsBox(sx / 2, sy / 2);

            t.body.CreateFixture(shape, 5);

            t.body.UserData = t.id;



            for (int x = 5; x < t.width - 5; x++)
            {
                for (int y = 5; y < t.height - 5; y++)
                {
                    t.cells[x, y] = Sampler.GetCell((short)MaterialType.Iron, GFX.Color.FromSDColor(bmp.GetPixel(x - 5, y - 5)));
                }
            }
            Tile.triangles += 2;
            return t;
        }
        public static Tile Create(World world, BodyType type, float px, float py, string image, Vector2[][] triangles)
        {
            Bitmap bmp = new Bitmap(image);

            int sx = bmp.Width;
            int sy = bmp.Height;

            count++;
            Tile t = new Tile(400, 400, sx + 10, sy + 10);
            t.id = count;
            LoopUp.Add(t.id, t);

            //t.midx = 0;
            //t.midy = 0;
            var bodyDef = new BodyDef
            {
                BodyType = type,
                AllowSleep = false,
                Position = new Vector2(px, py),
                GravityScale = 10
            };
            t.body = world.CreateBody(bodyDef);

            for (int i = 0; i < triangles.Length; i++)
            {           
                PolygonShape shape = new PolygonShape();
                shape.Set(triangles[i], triangles[i].Length);
                t.body.CreateFixture(shape, 1);

            }
 
 
            t.body.UserData = t.id;



            for (int x = 5; x < t.width - 5; x++)
            {
                for (int y = 5; y < t.height - 5; y++)
                {
                    t.cells[x, y] = Sampler.GetCell((short)MaterialType.Iron, GFX.Color.FromSDColor(bmp.GetPixel(x - 5, y - 5)));
                }
            }
            t.Poly = true;
            Tile.triangles += triangles.Length;
            return t;
        }
        public void Move(int x, int y)
        {
            this.nx = x;
            this.ny = y;
        }
        public void Rotate(float degrees)
        {
            this.nr = degrees * (float)Math.PI / 180f;
        }
        public void RotateRad(float radians)
        {
            this.nr = radians;
        }
        private static bool CloseEnough(float a, float b)
        {
            return Math.Abs(a - b) < 0.001f;
        }
        public void Tick(CellWorld world)
        {
            Vector2 pos = body.GetPosition();
            nr = body.GetAngle();
            body.IsAwake = !asleep;


            if (Poly)
            {
                double radius = Math.Sqrt((width * width) + (height * height)) / 2 - 6.28318530718f;
  

                nx = (int)Math.Round(pos.X - (Math.Cos(nr - 2.35619f) * radius) - (width / 2) - 6.28318530718f);
                ny = (int)Math.Round(pos.Y - (Math.Sin(nr - 2.35619f) * radius) - (height / 2) - 6.28318530718f);
            }
            else
            {
                nx = (int)Math.Round(pos.X) - width / 2 - 5;
                ny = (int)Math.Round(pos.Y) - height / 2 - 5;
            }


            if (!(world.InBounds(nx, ny) && world.InBounds(nx + width, ny + height)))
            {
                if (!asleep)
                {
                    CollectCells(world, x, y);
                    asleep = true;
                    init = true;

                    Debug.Log($"Tile {id} fell asleep.");
                }
                return;

            }
            else if (asleep)
            {
                asleep = false;
                Debug.Log($"Tile {id} awoke.");
            }

            if (x != nx || y != ny || !CloseEnough(rotation, nr) || init)
            {

                if (!init)
                    CollectCells(world, x, y);
                x = nx;
                y = ny;
                rotation = nr;
                InsertArray(world, nx, ny);
   
                init = false;

            }

        }
    }
}
