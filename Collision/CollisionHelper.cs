using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RayTracer.Geometry;

using Ray = RayTracer.Geometry.Ray;

namespace RayTracer.Collision
{
    public static class CollisionHelper
    {
        public static bool TestRaySphere(Ray r, Sphere s)
        {
            Vector3 m = r.Pos - s.Center;
            float c = Vector3.Dot(m, m) - s.Radius * s.Radius;

            if (c <= 0.0f)
            {
                return true;
            }

            float b = Vector3.Dot(m, r.Dir);

            if (b > 0.0f)
            {
                return false;
            }

            float disc = b * b - c;

            if (disc < 0.0f)
            {
                return false;
            }

            return true;
        }

        public static bool TestRaySphere(Ray r, Sphere s, out float t, out Vector3 q)
        {
            t = float.MaxValue;
            q = new Vector3();

            Vector3 m = r.Pos - s.Center;
            float b = Vector3.Dot(m, r.Dir);
            float c = Vector3.Dot(m, m) - s.Radius * s.Radius;

            if (c > 0.0f && b > 0.0f)
            {
                return false;
            }

            float discr = b * b - c;

            if (discr < 0.0f)
            {
                return false;
            }

            t = -b - (float)Math.Sqrt(discr);

            if (t < 0.0f)
            {
                t = 0.0f;
            }

            q = r.Pos + r.Dir * t;
            return true;
        }

        public static bool PointInSphere(Vector3 p, Sphere s)
        {
            return (p - s.Center).LengthSquared() <= s.Radius * s.Radius;
        }
    }
}
