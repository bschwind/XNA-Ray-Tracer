using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayTracer
{
    public struct Camera
    {
        public Matrix View, Proj, World;

        public Camera(Matrix view, Matrix proj, Matrix world)
        {
            View = view;
            Proj = proj;
            World = world;
        }
    }
}
