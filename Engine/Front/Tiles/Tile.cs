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
using FSEngine.Geometry;
using Box2DSharp.Common;

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
        public bool background = false;

        /// <summary>
        /// Resolution of the collider mesh, in percentage.
        /// </summary>
        public static float MeshResolution = 0.04f;
        /// <summary>
        /// How often the tile updates when not moving.
        /// Tick Rate of 1 is every frame, Tick Rate of 2 is every other frame; ect.
        /// </summary>
        public static int TickRate = 8;
        /// <summary>
        /// How many cells must change before the mesh updates.
        /// </summary>
        public static int EditTolerance = 10;

        bool Poly = false;
        int before = 0, after = 0;
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
            after = 0;
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
                            if(background && c.owner != (short)id)
                            {
                                if(c.type == 0)
                                {
                                    cells[_x, _y] = c;
                                    continue;
                                }

                                after++;
                                continue;
                            }

                            world.Clear(sx + x, sy + y);

                            if(c.owner == (short)id || c.type == 0)
                                cells[_x, _y] = c;
                                
       
                            if(c.type > 0)
                                    after++;
          
                        }
                    }
                    
                }
        }

        public void InsertArray(CellWorld world, int sx, int sy)
        {
            before = 0;
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
                            if(!(background && c.type > 0))
                            {
                                if (c.type > 0 && c.owner == 0)
                                {
                                    ParticleEngine.AddParticleWithVelocity(c, sx + x, sy + y);
                                }

                                world.SetCell(sx + x, sy + y, cells[_x, _y]);
                                before++;

 
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
                Position = new Vector2(px, py),
                GravityScale = 10
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

        private bool BadPolygon(Vector2[] vertices)
        {
            int count = vertices.Length;

            int num = Math.Min(count, 8);
            Span<Vector2> span = stackalloc Vector2[8];
            int num2 = 0;
            for (int i = 0; i < num; i++)
            {
                Vector2 vector = vertices[i];
                bool flag = true;
                for (int j = 0; j < num2; j++)
                {
                    if (Vector2.DistanceSquared(vector, span[j]) < 6.25E-06f)
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    span[num2] = vector;
                    num2++;
                }
            }

            num = num2;
            if (num < 3)
            {
                return true;
            }

            int num3 = 0;
            float num4 = span[0].X;
            for (int k = 1; k < num; k++)
            {
                float x = span[k].X;
                if (x > num4 || (x.Equals(num4) && span[k].Y < span[num3].Y))
                {
                    num3 = k;
                    num4 = x;
                }
            }

            Span<int> span2 = stackalloc int[8];
            int num5 = 0;
            int num6 = num3;
            int num7;
            do
            {
                span2[num5] = num6;
                num7 = 0;
                for (int l = 1; l < num; l++)
                {
                    if (num7 == num6)
                    {
                        num7 = l;
                        continue;
                    }

                    Vector2 a = span[num7] - span[span2[num5]];
                    Vector2 b = span[l] - span[span2[num5]];
                    float num8 = MathUtils.Cross(in a, in b);
                    if (num8 < 0f)
                    {
                        num7 = l;
                    }

                    if (num8.Equals(0f) && b.LengthSquared() > a.LengthSquared())
                    {
                        num7 = l;
                    }
                }

                num5++;
                num6 = num7;
            }
            while (num7 != num3);
            if (num5 < 3)
            {
                return true;
            }
            return false;
        }
        private void Remesh()
        {
            Tile.triangles -= body.FixtureList.Count;
            while (body.FixtureList.Count > 0)
            {
                body.DestroyFixture(body.FixtureList[0]);
            }

            Mesher m = new Mesher(null);
            Vector2[][] triangles = m.Mesh(this, MeshResolution);

            for (int i = 0; i < triangles.Length; i++)
            {
                if(!BadPolygon(triangles[i]))
                {
                    PolygonShape shape = new PolygonShape();
                    shape.Set(triangles[i], triangles[i].Length);
                    body.CreateFixture(shape, 1);
                }

            }

            Tile.triangles += triangles.Length;
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
                }
                return;

            }
            else if (asleep)
            {
                asleep = false;
            }

            if (x != nx || y != ny || !CloseEnough(rotation, nr) || init || world.frame % TickRate == 0)
            {

                if (!init)
                    CollectCells(world, x, y);
                x = nx;
                y = ny;
                rotation = nr; 

                if (Poly && !init && Math.Abs(before - after) > EditTolerance)
                {
                    if(body.BodyType == BodyType.DynamicBody)
                        Remesh();
                }

                InsertArray(world, nx, ny);

                init = false;

          
            }

        }
    }
}
