using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayTracer.Geometry
{
    public struct Sphere
    {
        public Vector3 Center;
        public float Radius;
        public Color Color;
        public float ReflectanceFactor;

        public Sphere(Vector3 center, float radius, Color c, float reflectanceFactor)
        {
            this.Center = center;
            this.Radius = radius;
            this.Color = c;
            this.ReflectanceFactor = reflectanceFactor;
        }
    }
}
