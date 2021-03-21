using UnityEngine;
using System;

namespace PostProcessSpace
{
    public class PostProcessFacade : MonoBehaviour, IPostProcessFacade
    {
        public event Action<RenderTexture> onDisplayUpdated;

        private enum Display
        {
            depth,
            normals,
            light,
        }

        private ILightingFacade lightingFacade;
        private PostProcessEffect pp_light;
        private PostProcessEffect pp_fxaa;
        private PostProcessEffect pp_depth;
        private bool antialiasing;
        private bool mustRender = false;

        [Header("References")]
        [SerializeField] Camera quadTreeCam;
        [SerializeField] RenderTexture depthTex;
        [SerializeField] RenderTexture normalTex;
        private RenderTexture displayDepthTex;
        private RenderTexture lightTex;
        private RenderTexture fxaaTex;

        
        private Display display = Display.light;

        private void Start()
        {
            lightingFacade = ServiceLocator.Instance.GetService<ILightingFacade>();

            CreateEffects();
            CreateTextures();

            lightingFacade.onChanged += PrepareLightShader;
            /*AbstractRenderer.onTexApplied += () =>
            {
                mustRender = true;
            };*/

            PrepareDepthShader();
            PrepareLightShader();
            PrepareFXAAShader(lightTex);
            UpdateDisplay();
        }

        private void Update()
        {
            if (mustRender)
            {
                mustRender = false;
                Render();
            }
        }

        #region Display

        public void DisplayDepth()
        {
            display = Display.depth;
            UpdateDisplay();
        }
        public void DisplayNormals()
        {
            display = Display.normals;
            UpdateDisplay();
        }
        public void DisplayLighting()
        {
            display = Display.light;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            RenderTexture displayTex = displayDepthTex;
            switch (display)
            {
                case Display.depth: displayTex = displayDepthTex; break;
                case Display.normals: displayTex = normalTex; break;
                case Display.light: displayTex = antialiasing ? fxaaTex : lightTex; break;
            }
            onDisplayUpdated(displayTex);
        }

        #endregion

        #region Creation

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
            displayDepthTex = CreateTex("displayDepth");
        }

        private void CreateEffects()
        {

            pp_fxaa = new PostProcessEffect("FXAAShader");
            pp_light = new PostProcessEffect("LightShader");
            pp_depth = new PostProcessEffect("DisplayDepthShader");
        }

        #endregion

        #region ShaderPreparation

        //bool fxaaShaderPrepared = false;
        private void PrepareFXAAShader(RenderTexture inputTex)
        {
            //fxaaShaderPrepared = true;
            pp_fxaa.shader.SetTexture(pp_fxaa.kernel, "InputTex", inputTex);
            pp_fxaa.shader.SetTexture(pp_fxaa.kernel, "ResultTex", fxaaTex);
            pp_fxaa.shader.SetInt("maxRes", fxaaTex.width);
        }

        //bool lightShaderPrepared = false;
        private void PrepareLightShader()
        {
            //lightShaderPrepared = true;
            pp_light.shader.SetFloat("fog", lightingFacade.IsFogActive() ? 1f : 0f);
            pp_light.shader.SetFloat("power", lightingFacade.GetFogPower());
            Vector3 lightDir = lightingFacade.GetLightDirVec();
            pp_light.shader.SetFloats("lightDir", new float[] { lightDir.x, lightDir.y, lightDir.z });
            pp_light.shader.SetTexture(pp_light.kernel, "ResultTex", lightTex);
            pp_light.shader.SetTexture(pp_light.kernel, "DepthTex", depthTex);
            pp_light.shader.SetTexture(pp_light.kernel, "NormalTex", normalTex);
        }

        private void PrepareDepthShader()
        {
            pp_depth.shader.SetTexture(pp_depth.kernel, "DepthTex", depthTex);
            pp_depth.shader.SetTexture(pp_depth.kernel, "ResultTex", displayDepthTex);
        }

        #endregion

        #region Rendering

        private void Render()
        {
            /*if (!fxaaShaderPrepared || !lightShaderPrepared)
                return;*/

            if (display == Display.light || display == Display.depth)
                RenderDepth();

            if (display == Display.light || display == Display.normals)
                RenderNormals();

            if (display == Display.light)
                RenderLight();
        }

        private void RenderDepth()
        {
            RendererManager.DisplayDepth();
            quadTreeCam.targetTexture = depthTex;
            quadTreeCam.Render();
            if (display == Display.depth)
            {
                pp_depth.Render(displayDepthTex.width, displayDepthTex.height);
            }
        }

        private void RenderNormals()
        {
            RendererManager.DisplayNormal();
            quadTreeCam.targetTexture = normalTex;
            quadTreeCam.Render();
        }

        private void RenderLight()
        {
            pp_light.Render(lightTex.width, lightTex.height);
            if (antialiasing)
            {
                pp_fxaa.Render(fxaaTex.width, fxaaTex.height);
            }
        }

        #endregion
    }
}
