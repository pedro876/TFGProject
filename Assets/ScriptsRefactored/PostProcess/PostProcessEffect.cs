using System.Collections;
using UnityEngine;

namespace PostProcessSpace
{
    public class PostProcessEffect
    {
        public ComputeShader shader;
        public int kernel;
        int[] numThreads;

        public PostProcessEffect(string shaderName, string kernelName = "CSMain")
        {
            shader = Resources.Load("Shaders/" + shaderName) as ComputeShader;
            kernel = shader.FindKernel(kernelName);
            uint x, y, z;
            shader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
            numThreads = new int[] { (int)x, (int)y, (int)z };
        }

        public void Render(int width, int height)
        {
            shader.Dispatch(kernel, width / numThreads[0], height / numThreads[1], numThreads[2]);
        }
    }
}