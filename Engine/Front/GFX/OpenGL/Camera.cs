using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace FSEngine.OpenGL
{
    public class Camera
    {
        public Vector2 Position;
        public float FOV;
        public Vector2 size;

        public Camera(Vector2 position, Single fOV, Vector2 size)
        {
            Position = position;
            FOV = fOV;
            this.size = size;
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            float top = Position.Y - size.Y / 2;
            float bottom = Position.Y + size.Y / 2;
            float left = Position.X - size.X / 2;
            float right = Position.X + size.X / 2;

            Matrix4x4 ortho = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0, 100);
            Matrix4x4 fov = Matrix4x4.CreateScale(FOV);
            return ortho * fov;
        }
    }
}
