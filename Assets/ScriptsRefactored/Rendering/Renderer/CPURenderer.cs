using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Collections.Generic;

namespace RenderingSpace
{
    public class CPURenderer : QuadRenderer
    {
        protected Color[] depthMemory;
        protected Color[] normalMemory;

        private Texture2D depthTex2D;
        private Texture2D normalTex2D;

        private Thread processorThread;

        #region Creation

        public CPURenderer(RawImage image, int level = 0, QuadRenderer parent = null) : base(image, level, parent) { }

        protected override void InitConcrete()
        {
            depthTex2D = CreateTex();
            depthTex = depthTex2D;

            normalTex2D = CreateTex();
            normalTex = normalTex2D;

            depthMemory = new Color[QuadInfo.resolution * QuadInfo.resolution];
            normalMemory = new Color[QuadInfo.resolution * QuadInfo.resolution];
        }

        private Texture2D CreateTex()
        {
            var tex = new Texture2D(QuadInfo.resolution, QuadInfo.resolution, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        #endregion

        #region rendering

        protected override void MemoryToTexConcrete()
        {
            depthTex2D.SetPixels(depthMemory);
            depthTex2D.Apply();

            normalTex2D.SetPixels(normalMemory);
            normalTex2D.Apply();
        }

        protected override void RenderInstant()
        {
            RenderRegion(0, 0, QuadInfo.resolution, QuadInfo.resolution);
            IsDone = true;
            IsRendering = false;
            MemoryToTex();
        }

        protected override void RenderMultiThread(int priority)
        {
            processorThread = new Thread(() =>
            {
                IsQueued = false;
                RenderRegion(0, 0, QuadInfo.resolution, QuadInfo.resolution);
                IsDone = true;
                lock (RenderingFacade.Instance.liveThreadsLock)
                {
                    RenderingFacade.Instance.liveThreads.Remove(Thread.CurrentThread);
                }
            }
            );
            RenderingFacade.Instance.queuedThreads.Add(new KeyValuePair<Thread, int>(processorThread, priority));
        }

        protected override void RenderSingleThread(int priority)
        {
            RenderingFacade.Instance.renderOrders.Add(new KeyValuePair<System.Action, int>(() =>
            {
                IsQueued = false;
                RenderRegion(0, 0, QuadInfo.resolution, QuadInfo.resolution);
                IsDone = true;
            }, priority));
        }

        #endregion

        #region RenderRegion

        private void RenderRegion(int minX, int minY, int maxX, int maxY)
        {
            GetCameraInfo(out var right, out var up, out var nearSize, out var farSize, out var nearStart, out var farStart);

            Ray ray = new Ray(QuadInfo.depthSamples);

            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {

                    Vector3 nearPos = nearStart + (right * ((float)x / QuadInfo.resolution) - up * ((float)y / QuadInfo.resolution)) * region * nearSize;
                    Vector3 farPos = farStart + (right * ((float)x / QuadInfo.resolution) - up * ((float)y / QuadInfo.resolution)) * region * farSize;

                    nearPos = regionFacade.TransformToRegion(ref nearPos);
                    farPos = regionFacade.TransformToRegion(ref farPos);

                    ray.SetOriginAndDestiny(ref nearPos, ref farPos);
                    ray.Cast(out bool landed, out float normDepth, out Color normalColor);

                    depths[x, y] = landed ? normDepth : 2f;
                    int row = QuadInfo.resolution - y - 1;
                    depthMemory[x + row * QuadInfo.resolution] = new Color(Mathf.Lerp(0f, 1f, normDepth), landed ? 1f : 0f, 0f, 1f);
                    normalMemory[x + row * QuadInfo.resolution] = normalColor;
                }
            }
        }

        #endregion

        #region Destroy

        protected override void Finish()
        {
            
        }

        #endregion

    }
}