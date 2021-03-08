using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostProcess : MonoBehaviour
{
    private class PostProcessEffect
    {
        public ComputeShader shader;
        public int kernel;
        int[] numThreads;

        public PostProcessEffect(string shaderName, string kernelName = "CSMain")
        {
            this.shader = Resources.Load("Shaders/" + shaderName) as ComputeShader;
            this.kernel = shader.FindKernel(kernelName);
            uint x, y, z;
            shader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
            numThreads = new int[] { (int)x, (int)y, (int)z };
        }

        public void Render(int width, int height)
        {
            shader.Dispatch(kernel, width / numThreads[0], height / numThreads[1], numThreads[2]);
        }
    }

    [SerializeField] Camera quadTreeCam;
    private RawImage functionView;

    //POST PROCESS EFFECTS
    private PostProcessEffect pp_light;
    private PostProcessEffect pp_fxaa;

    [Header("Render textures")]
    [SerializeField] RenderTexture depthTex;
    [SerializeField] RenderTexture normalTex;
    private RenderTexture lightTex;
    private RenderTexture fxaaTex;

    [Header("Visualization Options")]
    [SerializeField] bool fog;
    [SerializeField] bool antialiasing;

    public enum Display
    {
        depth,
        normals,
        light,
    }
    [SerializeField] Display display = Display.normals;

    private void Start()
    {
        functionView = GameObject.FindGameObjectWithTag("FunctionView").GetComponent<RawImage>();

        CreateEffects();
        CreateTextures();

        Renderer.onTexApplied += Render;
        UpdateDisplay();
    }

    private void Update()
    {
        UpdateDisplay();
    }

    private void CreateTextures()
    {
        RenderTexture CreateTex(string name)
        {
            RenderTexture rt = new RenderTexture(depthTex.width, depthTex.height, 24);
            rt.name = name;
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }

        lightTex = CreateTex("lightTex");
        fxaaTex = CreateTex("fxaaTex");
    }

    private void CreateEffects()
    {
        pp_fxaa = new PostProcessEffect("FXAAShader");
        pp_light = new PostProcessEffect("LightShader");
    }

    private void PrepareFXAAShader(RenderTexture inputTex)
    {
        pp_fxaa.shader.SetTexture(pp_fxaa.kernel, "InputTex", inputTex);
        pp_fxaa.shader.SetTexture(pp_fxaa.kernel, "ResultTex", fxaaTex);
        pp_fxaa.shader.SetInt("maxRes", fxaaTex.width);
    }

    private void PrepareLightShader()
    {
        pp_light.shader.SetTexture(pp_light.kernel, "ResultTex", lightTex);
        pp_light.shader.SetTexture(pp_light.kernel, "DepthTex", depthTex);
        pp_light.shader.SetTexture(pp_light.kernel, "NormalTex", normalTex);
    }

    private void UpdateDisplay()
    {

        switch (display)
        {
            case Display.depth: functionView.texture = depthTex; break;
            case Display.normals: functionView.texture = normalTex; break;
            case Display.light: functionView.texture = antialiasing ? fxaaTex : lightTex; break;
        }
        
        Render();
    }

    private void Render()
    {
        if (display == Display.light || display == Display.depth)
        {
            RendererManager.DisplayDepth();
            quadTreeCam.targetTexture = depthTex;
            quadTreeCam.Render();
        }
        
        if(display == Display.light || display == Display.normals)
        {
            RendererManager.DisplayNormal();
            quadTreeCam.targetTexture = normalTex;
            quadTreeCam.Render();
        }

        if (display == Display.light)
        {
            PrepareLightShader();
            pp_light.Render(lightTex.width, lightTex.height);
            if (antialiasing)
            {
                PrepareFXAAShader(lightTex);
                pp_fxaa.Render(fxaaTex.width, fxaaTex.height);
            }
        }

        
    }
}
