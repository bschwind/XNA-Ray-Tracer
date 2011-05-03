using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracer.Lights
{
    public class IntermediateLightData
    {
        public Vector3 Color;
        public float ReflectanceFraction; //Determines how much reflection factors in to the final color

        public IntermediateLightData(Vector3 c, float r)
        {
            Color = c;
            ReflectanceFraction = r;
        }
    }
}
