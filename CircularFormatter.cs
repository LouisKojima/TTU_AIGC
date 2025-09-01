using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace IAVTools
{
    /// <summary>
    /// Tool class for generating ring textures
    /// </summary>
    public class CircularFormatter 
    {
        private CircularFormatter() { }
        /// <summary>
        /// Generates a ring texture of transparent background and counter-clockwise color fading.
        /// </summary>
        /// <param name="radius">Outer raidus of the ring</param>
        /// <param name="width">Width of the ring</param>
        /// <param name="angle">Arc length of the ring (0 ~ 360)</param>
        /// <param name="color">Color of the ring</param>
        /// <param name="radial">True if the fading goes radially</param>
        /// <returns>Generated texture</returns>
        public static Texture2D GenerateArcTexture(int radius, int width, int angle, Color color, bool radial = false)
        {
            Gradient gradient = new();
            GradientColorKey[] c = { new(color, 0f), new(color, 1f) };
            GradientAlphaKey[] a = { new(1f, 0f), new(0f, 1f) };
            gradient.SetKeys(c,a);
            return GenerateArcTexture(radius, width, angle, gradient, Vector2.up, radial);
        }
        /// <summary>
        /// Generates a ring texture of transparent background and gradient.
        /// </summary>
        /// <param name="radius">Outer raidus of the ring</param>
        /// <param name="width">Width of the ring</param>
        /// <param name="angle">Arc length of the ring (0 ~ 360)</param>
        /// <param name="gradient">Gradient of the ring</param>
        /// <param name="radial">True if the gradient goes radially</param>
        /// <returns>Generated texture</returns>
        public static Texture2D GenerateArcTexture(int radius, int width, int angle, Gradient gradient, bool radial = false)
        {
            return GenerateArcTexture(radius, width, angle, gradient,Vector2.up, radial);
        }
        /// <summary>
        /// Generates a ring texture of transparent background and gradient, specifies 0 degree axis.
        /// </summary>
        /// <param name="radius">Outer raidus of the ring</param>
        /// <param name="width">Width of the ring</param>
        /// <param name="angle">Arc length of the ring (0 ~ 360)</param>
        /// <param name="gradient">Gradient of the ring</param>
        /// <param name="radial">True if the gradient goes radially</param>
        /// <param name="axis">The direction of 0 angle</param>
        /// <returns>Generated texture</returns>
        public static Texture2D GenerateArcTexture(int radius, int width, int angle, Gradient gradient, Vector2 axis, bool radial = false)
        {
            int diameter = radius * 2;
            Texture2D texture = new(diameter, diameter, TextureFormat.ARGB32, false);
            for (int Oy = 0; Oy < diameter; Oy++)
            {
                Color[] row = new Color[diameter];
                //Generate pixels
                row = row.Select((c, Ox) =>
                {
                    //Move the origin from corner to the center for easier calculation
                    Vector2 coordinate = new(Ox - radius, Oy - radius);
                    float x = coordinate.x;
                    float y = coordinate.y;
                    //Polar coordinate (theta <= angle / 2f || theta >= 360 - angle / 2f)
                    float theta = Vector2.SignedAngle(-axis, coordinate) + 180;
                    float r = coordinate.magnitude;
                    //Math expresstion for the ring
                    bool condition =
                        r >= radius - width &&
                        r <= radius &&
                        theta <= angle;
                    //Color gradient accoring to coordinate
                    if (condition)
                    {
                        if (radial) return gradient.Evaluate((r - radius + width) / width);
                        return gradient.Evaluate(theta / angle);
                    }
                    //Transparent background
                    return Color.clear;
                }).ToArray();
                texture.SetPixels(0, Oy, diameter, 1, row);
            }
            //Make transparent in editor
#if UNITY_EDITOR
            texture.alphaIsTransparency = true;
#endif
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Save a texture as PNG according to path and name
        /// </summary>
        /// <param name="texture">Texture to save</param>
        /// <param name="absPath">Absolute file path</param>
        /// <param name="name">File name</param>
        public static void Save2PNG(Texture2D texture, string absPath, string name)
        {
            byte[] bytes = texture.EncodeToPNG();
            if (!absPath.EndsWith("/")) absPath += "/";
            if (!Directory.Exists(absPath))
            {
                Directory.CreateDirectory(absPath);
            }
            File.WriteAllBytes(absPath + name + ".png", bytes);
        }
    }
}
