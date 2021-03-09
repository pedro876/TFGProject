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

    public static bool antialiasing = true;
    public static bool fog = true;
    public static float fogPower = 3f;
    private static Vector3 lightDir = new Vector3(1, -1, 0.5f).normalized;

    public enum Display
    {
        depth,
        normals,
        light,
    }
    public static Display display = Display.light;

    private void Start()
    {
        functionView = GameObject.FindGameObjectWithTag("FunctionView").GetComponent<RawImage>();

        CreateEffects();
        CreateTextures();

        Renderer.onTexApplied += Render;
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
        pp_light.shader.SetFloat("fog", fog ? 1f : 0f);
        pp_light.shader.SetFloat("power", fogPower);
        pp_light.shader.SetFloats("lightDir", new float[] { lightDir.x, lightDir.y, lightDir.z });
        pp_light.shader.SetTexture(pp_light.kernel, "ResultTex", lightTex);
        pp_light.shader.SetTexture(pp_light.kernel, "DepthTex", depthTex);
        pp_light.shader.SetTexture(pp_light.kernel, "NormalTex", normalTex);
    }

    public void UpdateDisplay()
    {
        switch (display)
        {
            case Display.depth: functionView.texture = depthTex; break;
            case Display.normals: functionView.texture = normalTex; break;
            case Display.light: functionView.texture = antialiasing ? fxaaTex : lightTex; break;
        }
        
        Render();
    }

    public void Render()
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

    #region LightDir

    public static void SetLightDir(float rotation, float inclination)
    {
        lightDir = Vector3.right;
        lightDir = Quaternion.AngleAxis(inclination, Vector3.forward) * lightDir;
        lightDir = Quaternion.AngleAxis(rotation, Vector3.up) * lightDir;
        lightDir.Normalize();
    }

    public static Vector2 GetLightDir()
    {
        lightDir.Normalize();
        Vector3 projUp = Vector3.ProjectOnPlane(lightDir, Vector3.up).normalized;
        float rotation = Vector3.SignedAngle(Vector3.right, projUp, Vector3.up);

        Vector3 axis = Vector3.Cross(lightDir, projUp);
        float inclination = Vector3.SignedAngle(projUp, lightDir, axis);
        return new Vector2(rotation, inclination);
    }

    #endregion
}
