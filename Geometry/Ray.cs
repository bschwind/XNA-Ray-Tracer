using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayTracer.Geometry
{
    public struct Ray
    {
        public Vector3 Pos;
        public Vector3 Dir;

        public Ray(Vector3 pos, Vector3 dir)
        {
            this.Pos = pos;
            this.Dir = dir;
        }
    }
}
