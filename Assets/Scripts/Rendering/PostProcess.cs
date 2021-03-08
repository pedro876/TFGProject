using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostProcess : MonoBehaviour
{
    [SerializeField] Camera quadTreeCam;
    private RawImage functionView;

    [SerializeField] ComputeShader postProcessShader;
    int[] numThreads = new int[3];
    int normalKernel;
    private RenderTexture postprocessTex;

    [SerializeField] RenderTexture depthTex;
    [SerializeField] RenderTexture normalTex;

    private void Start()
    {
        postprocessTex = new RenderTexture(depthTex.width, depthTex.height, 24);
        postprocessTex.name = "postProcessTex";
        postprocessTex.enableRandomWrite = true;
        postprocessTex.Create();
        functionView = GameObject.FindGameObjectWithTag("FunctionView").GetComponent<RawImage>();
        functionView.texture = postprocessTex;


        normalKernel = postProcessShader.FindKernel("SunLight");
        uint x, y, z;
        postProcessShader.GetKernelThreadGroupSizes(normalKernel, out x, out y, out z);
        numThreads[0] = (int) x;
        numThreads[1] = (int) y;
        numThreads[2] = (int) z;

        postProcessShader.SetTexture(normalKernel, "ResultTex", postprocessTex);
        postProcessShader.SetTexture(normalKernel, "DepthTex", depthTex);
        postProcessShader.SetTexture(normalKernel, "NormalTex", normalTex);
    }

    private void Update()
    {
        RendererManager.DisplayDepth();
        quadTreeCam.targetTexture = depthTex;
        quadTreeCam.Render();
        RendererManager.DisplayNormal();
        quadTreeCam.targetTexture = normalTex;
        quadTreeCam.Render();

        postProcessShader.Dispatch(normalKernel, postprocessTex.width / numThreads[0], postprocessTex.height / numThreads[1], numThreads[2]);

    }
}
