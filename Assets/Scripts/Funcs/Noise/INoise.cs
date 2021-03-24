using System.Collections;
using UnityEngine;

namespace FuncSpace
{
    public interface INoise
    {
        /*float GetAuxValueForRandom();
        Vector2 GetAuxVec2ForRandom();
        Vector3 GetAuxVec3ForRandom();*/
        int GetStreamSize();
        float[] GetRandomStream();

        float Random(float x);
        float Random(float x, float y);
        float Random(float x, float y, float z);

        float Voxel(float x, float y, float z);

        float Perlin(float x);

        float Perlin(float x, float y);
        float Perlin(float x, float y, float z);
    }
}