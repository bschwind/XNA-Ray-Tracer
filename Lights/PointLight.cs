using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracer.Lights
{
    public class PointLight
    {
        public Vector3 Pos;
        public Color Color;

        public PointLight(Vector3 pos, Color color)
        {
            Pos = pos;
            Color = color;
        }
    }
}
