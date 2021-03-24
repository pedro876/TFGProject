using System.Collections;
using UnityEngine;

namespace FuncSpace
{
    public class Noise : INoise
    {
        private const int seed = 69;
        private const int streamSize = 1024;
        private float[] randomStream;
        private static readonly float auxValueForRandom = 43758.54f;
        private static readonly Vector2 auxVec2ForRandom = new Vector2(12.9898f, 78.233f);
        private static readonly Vector3 auxVec3ForRandom = new Vector3(12.9898f, 78.233f, 43.7689f);

        public Noise()
        {
            FillRandomStream();
        }

        private void FillRandomStream()
        {
            System.Random rnd = new System.Random(seed);
            randomStream = new float[streamSize];
            for (int i = 0; i < streamSize; i++)
            {
                randomStream[i] = (float)rnd.NextDouble();
            }
        }

        /*public float GetAuxValueForRandom() => auxValueForRandom;
        public Vector2 GetAuxVec2ForRandom() => auxVec2ForRandom;
        public Vector3 GetAuxVec3ForRandom() => auxVec3ForRandom;*/
        public int GetStreamSize() => streamSize;
        public float[] GetRandomStream() => randomStream;

        public float Random(float x)
        {
            return randomStream[((int)(Mathf.Abs(Mathf.Sin(x)) * 5000)) % streamSize];
        }

        public float Random(float x, float y)
        {
            return Random((x * auxVec2ForRandom.x + y * auxVec2ForRandom.y) * auxValueForRandom);
        }

        public float Random(float x, float y, float z)
        {
            return Random((x * auxVec3ForRandom.x + y * auxVec3ForRandom.y + z * auxVec3ForRandom.z) * auxValueForRandom);
        }

        public float Voxel(float x, float y, float z)
        {
            return Random(Mathf.Round(x), Mathf.Round(y), Mathf.Round(z));
        }

        public float Perlin(float x)
        {
            x += 1000f;
            float fract = Fract(x);
            x = Mathf.Floor(x);
            return Smooth(Random(x), Random(x+1f), fract);
        }

        public float Perlin(float x, float y)
        {
            x += 1000f;
            y += 1000f;
            float xFract = Fract(x);
            float yFract = Fract(y);

            float x0 = Mathf.Floor(x);
            float y0 = Mathf.Floor(y);
            float x1 = x0 + 1f;
            float y1 = y0 + 1f;

            float c00 = Random(x0, y0);
            float c10 = Random(x1, y0);
            float c01 = Random(x0, y1);
            float c11 = Random(x1, y1);

            float s00_10 = Smooth(c00, c10, xFract);
            float s01_11 = Smooth(c01, c11, xFract);

            float perlin2 = Smooth(s00_10, s01_11, yFract);

            return perlin2;
        }

        public float Perlin(float x, float y, float z)
        {
            x += 1000f;
            y += 1000f;
            z += 1000f;
            float xFract = Fract(x);
            float yFract = Fract(y);
            float zFract = Fract(z);

            float x0 = Mathf.Floor(x);
            float y0 = Mathf.Floor(y);
            float z0 = Mathf.Floor(z);
            float x1 = x0 + 1f;
            float y1 = y0 + 1f;
            float z1 = z0 + 1f;

            float c000 = Random(x0, y0, z0);
            float c100 = Random(x1, y0, z0);
            float c010 = Random(x0, y1, z0);
            float c110 = Random(x1, y1, z0);

            float c001 = Random(x0, y0, z1);
            float c101 = Random(x1, y0, z1);
            float c011 = Random(x0, y1, z1);
            float c111 = Random(x1, y1, z1);

            float s000_100 = Smooth(c000, c100, xFract);
            float s010_110 = Smooth(c010, c110, xFract);

            float perlinZ0 = Smooth(s000_100, s010_110, yFract);

            float s001_101 = Smooth(c001, c101, xFract);
            float s011_111 = Smooth(c011, c111, xFract);

            float perlinZ1 = Smooth(s001_101, s011_111, yFract);

            float perlin3 = Smooth(perlinZ0, perlinZ1, zFract);

            return perlin3;
        }

        private float Fract(float f)
        {
            return f - Mathf.Floor(f);
        }

        private float Smooth(float min, float max, float v)
        {
            v = Mathf.Clamp(v * v * (3 - 2 * v), 0, 1);
            return v * (max - min) + min;
        }
    }
}