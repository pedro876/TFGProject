using UnityEngine;
using System;

namespace PostProcessSpace
{
    public class PostProcessFacade : MonoBehaviour, IPostProcessFacade
    {
        public event Action onDisplayUpdated;

        public RenderTexture DisplayTexture { get; private set; }

        private enum Display
        {
            Depth,
            Normals,
            Light,
        }

        private ILightingFacade lightingFacade;
        private IRenderingFacade renderingFacade;
        private IViewFacade viewFacade;
        private PostProcessEffect pp_light;
        private PostProcessEffect pp_fxaa;
        private PostProcessEffect pp_depth;
        private bool antialiasing = true;
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
            viewFacade = ServiceLocator.Instance.GetService<IViewFacade>();

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
            viewFacade.onChanged += () =>
            {
                PrepareViewParams();
                mustRender = true;
            };
            renderingFacade.onQuadRendered += () => mustRender = true;
            Render();
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

        public bool IsUsingAntialiasing()
        {
            return antialiasing;
        }

        public bool IsDisplayinDepth()
        {
            return display == Display.Depth;
        }

        public bool IsDisplayinNormals()
        {
            return display == Display.Normals;
        }

        public bool IsDisplayinLighting()
        {
            return display == Display.Light;
        }

        public void UseAntialiasing(bool use)
        {
            antialiasing = use;
            UpdateDisplay();
        }

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
            switch (display)
            {
                case Display.Depth: DisplayTexture = displayDepthTex; break;
                case Display.Normals: DisplayTexture = normalTex; break;
                case Display.Light: DisplayTexture = antialiasing ? fxaaTex : lightTex; break;
            }
            onDisplayUpdated?.Invoke();
            mustRender = true;
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

        private void PrepareViewParams()
        {
            Vector3 viewDir = viewFacade.Direction;
            pp_light.shader.SetFloats("viewDir", new float[] { viewDir.x, viewDir.y, viewDir.z });
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
            renderingFacade.DisplayDepth();
            quadTreeCam.targetTexture = depthTex;
            quadTreeCam.Render();
            if (display == Display.Depth)
            {
                pp_depth.Render(displayDepthTex.width, displayDepthTex.height);
            }
        }

        private void RenderNormals()
        {
            renderingFacade.DisplayNormals();
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
