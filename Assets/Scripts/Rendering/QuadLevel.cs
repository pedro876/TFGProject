using System.Collections;
using UnityEngine;

namespace RenderingSpace
{
    public class QuadLevel
    {
        public int resolution;
        public int depthSamples;

        public QuadLevel(int resolution, int depthSamples)
        {
            this.resolution = resolution;
            this.depthSamples = depthSamples;
        }
    }
}