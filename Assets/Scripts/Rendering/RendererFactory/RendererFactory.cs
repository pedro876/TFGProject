using UnityEngine;
using UnityEngine.UI;

namespace RenderingSpace
{
    public class RendererFactory : IRendererFactory
    {
        private GameObject quadImageProto;
        private RectTransform quadContainer;

        public RendererFactory()
        {
            quadImageProto = Resources.Load("Objects/quadTex") as GameObject;
            quadContainer = GameObject.FindGameObjectWithTag("QuadContainer").GetComponent<RectTransform>();
        }

        public IRenderer CreateCPURenderer(int level = 0, int childIndex = 0, QuadRenderer parent = null)
        {
            var image = CreateTexObject(level, parent);
            var renderer = new CPURenderer(image, level, childIndex, parent);
            renderer.QuadInfo = RenderConfig.cpuSetting[level];
            renderer.Init();

            if (level < RenderConfig.cpuSetting.Length - 1)
            {
                var children = new CPURenderer[4];
                for (int i = 0; i < 4; i++)
                {
                    children[i] = (CPURenderer)CreateCPURenderer(level + 1, i, renderer);
                }
                renderer.Children = children;
            }

            return renderer;
        }
        public IRenderer CreateGPURenderer(int level = 0, int childIndex = 0, QuadRenderer parent = null)
        {
            var image = CreateTexObject(level, parent);
            var renderer = new GPURenderer(image, level, childIndex, parent);
            renderer.QuadInfo = RenderConfig.gpuSetting[level];
            renderer.Init();

            if (level < RenderConfig.gpuSetting.Length - 1)
            {
                var children = new GPURenderer[4];
                for (int i = 0; i < 4; i++)
                {
                    children[i] = (GPURenderer)CreateGPURenderer(level + 1, i, renderer);
                }
                renderer.Children = children;
            }

            return renderer;
        }

        private RawImage CreateTexObject(int level, QuadRenderer parent = null)
        {
            var imageObj = GameObject.Instantiate(quadImageProto, level == 0 ? quadContainer : parent.ImageTransform);
            imageObj.name = "tex" + level + "_" + ((level != 0) ? parent.ImageTransform.childCount.ToString() : "");
            var imageTrasform = imageObj.GetComponent<RectTransform>();
            if (level == 0)
            {
                imageObj.transform.SetAsFirstSibling();
                imageTrasform.position = quadContainer.position;
                imageTrasform.sizeDelta = quadContainer.sizeDelta;
            }
            var image = imageObj.GetComponent<RawImage>();
            image.enabled = false;
            return image;
        }

        
    }
}