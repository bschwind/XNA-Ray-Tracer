using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RayTracer.Geometry;
using RayTracer.Collision;
using RayTracer.Lights;
using System.Threading;

using Ray = RayTracer.Geometry.Ray;

namespace RayTracer
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Ray[] eyeRays;
        Sphere[] spheres;
        Camera cam;
        Color[] pixels;
        PointLight[] lights;
        Texture2D results;

        float ambientCoefficient = 0.1f;

        int iterations = 10;
        int numLights = 2;
        int numSpheres = 10;
        int width = 512;
        int height = 512;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.PreferMultiSampling = true;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Vector3 camPos = new Vector3(0, 1, 1);
            Vector3 camTarget = new Vector3(0, 1, 0);
            cam = new Camera();
            cam.Proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), width / height, 0.1f, 1000f);
            cam.View = Matrix.CreateLookAt(camPos, camTarget, Vector3.Up);
            cam.World = Matrix.CreateTranslation(camPos);
            
            InitializeEyeRays(cam);
            InitializeSpheres(numSpheres);
            InitializeTextures();
            InitializeLights(numLights);
            Thread t = new Thread(new ThreadStart(RayTrace));
            t.Start();

            //RayTrace();

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private void RayTrace()
        {
            Vector3 q = Vector3.Zero;
            float t;
            Stack<IntermediateLightData> ildStack = new Stack<IntermediateLightData>(iterations);

            for (int j = 0; j < width; j++)
            {
                for (int i = 0; i < height; i++)
                {
                    ildStack.Clear();
                    Vector3 final = Vector3.Zero; //The final color for this pixel
                    int index = j * width + i;    //The index of this pixel in our array
                    
                    Ray r = eyeRays[index];
                    for (int k = 0; k < iterations; k++)
                    {
                        //Find r's intersect
                        float minT = float.MaxValue;
                        Sphere minSphere = new Sphere();
                        Vector3 hitPoint = Vector3.Zero;

                        for (int m = 0; m < spheres.Length; m++)
                        {
                            bool hit = CollisionHelper.TestRaySphere(r, spheres[m], out t, out q);
                            if (hit && t < minT)
                            {
                                minT = t;
                                minSphere = spheres[m];
                                hitPoint = q;
                            }
                        }

                        //If we didn't intersect anything, push black on the stack and break out of the loop
                        if (minT >= float.MaxValue)
                        {
                            ildStack.Push(new IntermediateLightData(Vector3.Zero, 0.0f));
                            break;
                        }
                        else
                        {
                            //Else we intersected something, namely minSphere
                            //Sum up the light contribution, and push our color value on the stack
                            Vector3 lightSum = Vector3.Zero;
                            Vector3 normal = Vector3.Normalize(hitPoint - minSphere.Center);
                            for(int n = 0; n < lights.Length; n++)
                            {
                                bool inShadow = false;
                                Vector3 lightDir = lights[n].Pos - hitPoint;
                                //If the surface isn't facing the light to begin with...
                                if (Vector3.Dot(normal, lightDir) <= 0)
                                {
                                    inShadow = true;
                                    break;
                                }
                                if (CollisionHelper.PointInSphere(lights[n].Pos, minSphere))
                                {
                                    inShadow = true;
                                    break;
                                }

                                //Check to see if we can reach the light
                                float pointToLightSquared = lightDir.LengthSquared();
                                lightDir.Normalize();
                                Ray pointToLight = new Ray(hitPoint + normal * 0.001f, lightDir);         
                                float tTest = float.MaxValue;
                                Vector3 newHitPoint = new Vector3();

                                for (int z = 0; z < spheres.Length; z++)
                                {
                                    bool intersects = CollisionHelper.TestRaySphere(pointToLight, spheres[z], out tTest, out newHitPoint);
                                    //If we intersected a sphere before we hit the light, we're in shadow
                                    if (intersects && tTest * tTest < pointToLightSquared)
                                    {
                                        inShadow = true;
                                        break;
                                    }
                                }
                                if (!inShadow)
                                {
                                    float attenuation = (hitPoint - lights[n].Pos).LengthSquared();
                                    attenuation = Vector3.Dot(normal, pointToLight.Dir);
                                    lightSum += lights[n].Color.ToVector3() * attenuation;
                                }
                            }
                            ildStack.Push(new IntermediateLightData(minSphere.Color.ToVector3() * ambientCoefficient + lightSum, minSphere.ReflectanceFactor));
                            
                            Vector3 newDir = Vector3.Reflect(r.Dir, normal);
                            newDir.Normalize();
                            r = new Ray(hitPoint + normal * 0.001f, newDir);

                            pixels[index] = new Color(new Vector4(minSphere.Color.ToVector3() * ambientCoefficient + lightSum, 1));
                        }
                    }

                    final = ildStack.Pop().Color;
                    //ildStack now has our color data
                    while (ildStack.Count > 0)
                    {
                        IntermediateLightData ild = ildStack.Pop();
                        final = ild.Color + (ild.ReflectanceFraction * final);
                    }

                    Color finalColor = Color.FromNonPremultiplied(new Vector4(final, 1f));
                    pixels[index] = Color.FromNonPremultiplied(new Vector4(final, 1f));
                }
            }
            
        }

        private void InitializeLights(int lightCount)
        {
            lights = new PointLight[lightCount];
            Random r = new Random();

            for (int i = 0; i < lights.Length; i++)
            {
                lights[i] = new PointLight(new Vector3(r.Next(-10, 10), r.Next(-20,20), r.Next(-20, -10)),
                                           new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), 1));

            }
        }

        private void InitializeTextures()
        {
            pixels = new Color[width * height];
            results = new Texture2D(GraphicsDevice, width, height);
        }

        private void InitializeSpheres(int sphereCount)
        {
            spheres = new Sphere[sphereCount];
            Random r = new Random();

            for (int i = 0; i < sphereCount; i++)
            {
                Sphere s = new Sphere();
                s.Radius = r.Next(1, 6);
                s.Center = new Vector3(r.Next(-10, 10), r.Next(-3, 6), r.Next(-40, -15));
                s.Color = Color.FromNonPremultiplied(new Vector4((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), 1));
                s.ReflectanceFactor = 0.8f;

                spheres[i] = s;
            }
        }

        private void InitializeEyeRays(Camera camera)
        {
            eyeRays = new Ray[width * height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Vector3 source1 = new Vector3(i, j, 0f);
                    Vector3 source2 = new Vector3(i, j, 1f);
                    Vector3 unproj1 = GraphicsDevice.Viewport.Unproject(source1, cam.Proj, cam.View, cam.World);
                    Vector3 unproj2 = GraphicsDevice.Viewport.Unproject(source2, cam.Proj, cam.View, cam.World);

                    Ray r = new Ray();
                    r.Pos = unproj1;
                    r.Dir = Vector3.Normalize(unproj2-unproj1);

                    eyeRays[(j * width) + i] = r;
                }
            }
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                LoadContent();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.Textures[0] = null;
            results.SetData<Color>(pixels);

            spriteBatch.Begin();
            spriteBatch.Draw(results, Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
