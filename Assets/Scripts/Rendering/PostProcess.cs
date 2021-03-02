using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostProcess : MonoBehaviour
{
    [SerializeField] Camera depthCam;
    private RawImage functionView;

    int[] numThreads = new int[3];
    [SerializeField] ComputeShader normalShader;
    int normalKernel;
    private RenderTexture normalTex;

    private void Start()
    {
        normalTex = new RenderTexture(depthCam.pixelWidth, depthCam.pixelHeight, 24);
        normalTex.name = "normalTex";
        normalTex.enableRandomWrite = true;
        normalTex.Create();
        functionView = GameObject.FindGameObjectWithTag("FunctionView").GetComponent<RawImage>();
        functionView.texture = normalTex;


        normalKernel = normalShader.FindKernel("Normals");
        uint x, y, z;
        normalShader.GetKernelThreadGroupSizes(normalKernel, out x, out y, out z);
        numThreads[0] = (int) x;
        numThreads[1] = (int) y;
        numThreads[2] = (int) z;

        normalShader.SetTexture(normalKernel, "ResultTex", normalTex);
        normalShader.SetTexture(normalKernel, "DepthTex", depthCam.targetTexture);
        normalShader.SetFloat("maxRes", normalTex.width);
    }

    private void Update()
    {
        depthCam.Render();

        normalShader.Dispatch(normalKernel, normalTex.width / numThreads[0], normalTex.height / numThreads[1], numThreads[2]);

    }
}
