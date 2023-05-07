using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.OpenGL
{
    public class Transform
    {
        public float rotation;
        public Vector2 position;
        public Vector2 scale;

        public Transform(Single rotation, Vector2 position, Vector2 scale)
        {
            this.rotation = rotation;
            this.position = position;
            this.scale = scale;
        }

        public Matrix4x4 GetModelMatrix()
        {

            float rotation = this.rotation / 360 *(float) Math.PI * 2f;

            Matrix4x4 rot = Matrix4x4.CreateRotationZ(rotation);
            Matrix4x4 sca = Matrix4x4.CreateScale(new Vector3(scale, 1));
            Matrix4x4 pos = Matrix4x4.CreateTranslation(new Vector3(position, 1));

            return sca * rot * pos;
        }
    }
}
