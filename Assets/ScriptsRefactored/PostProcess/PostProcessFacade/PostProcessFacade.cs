using UnityEngine;
using System;

namespace PostProcessSpace
{
    public class PostProcessFacade : MonoBehaviour, IPostProcessFacade
    {
        public event Action<RenderTexture> onDisplayUpdated;

        private enum Display
        {
            Depth,
            Normals,
            Light,
        }

        private ILightingFacade lightingFacade;
        private IRenderingFacade renderingFacade;
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

        
        private Display display = Display.Light;

        private void Start()
        {
            lightingFacade = ServiceLocator.Instance.GetService<ILightingFacade>();
            renderingFacade = ServiceLocator.Instance.GetService<IRenderingFacade>();

            CreateEffects();
            CreateTextures();
            PrepareDepthShader();
            PrepareLightShader();
            PrepareFXAAShader();
            UpdateDisplay();

            lightingFacade.onChanged += () =>
            {
                PrepareLightShader();
                mustRender = true;
            };
            renderingFacade.onQuadRendered += () => mustRender = true;
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
            display = Display.Depth;
            UpdateDisplay();
        }
        public void DisplayNormals()
        {
            display = Display.Normals;
            UpdateDisplay();
        }
        public void DisplayLighting()
        {
            display = Display.Light;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            RenderTexture displayTex = displayDepthTex;
            switch (display)
            {
                case Display.Depth: displayTex = displayDepthTex; break;
                case Display.Normals: displayTex = normalTex; break;
                case Display.Light: displayTex = antialiasing ? fxaaTex : lightTex; break;
            }
            onDisplayUpdated?.Invoke(displayTex);
        }

        #endregion

        #region Creation

        private void CreateTextures()
        {
            lightTex = CreateTex("lightTex");
            fxaaTex = CreateTex("fxaaTex");
            displayDepthTex = CreateTex("displayDepth");
        }

        private RenderTexture CreateTex(string name)
        {
            RenderTexture rt = new RenderTexture(depthTex.width, depthTex.height, 24);
            rt.name = name;
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }

        private void CreateEffects()
        {

            pp_fxaa = new PostProcessEffect("FXAAShader");
            pp_light = new PostProcessEffect("LightShader");
            pp_depth = new PostProcessEffect("DisplayDepthShader");
        }

        #endregion

        #region ShaderPreparation

        private void PrepareFXAAShader()
        {
            pp_fxaa.shader.SetTexture(pp_fxaa.kernel, "InputTex", lightTex);
            pp_fxaa.shader.SetTexture(pp_fxaa.kernel, "ResultTex", fxaaTex);
            pp_fxaa.shader.SetInt("maxRes", fxaaTex.width);
        }

        private void PrepareLightShader()
        {
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
            if (display == Display.Light || display == Display.Depth)
                RenderDepth();

            if (display == Display.Light || display == Display.Normals)
                RenderNormals();

            if (display == Display.Light)
                RenderLight();
        }

        private void RenderDepth()
        {
            RendererManager.DisplayDepth();
            quadTreeCam.targetTexture = depthTex;
            quadTreeCam.Render();
            if (display == Display.Depth)
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
