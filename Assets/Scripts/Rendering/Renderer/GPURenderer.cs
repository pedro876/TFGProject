using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace RenderingSpace
{
    public class GPURenderer : QuadRenderer
    {
        public GPURenderer(RawImage image, int level = 0, int childIndex = 0, QuadRenderer parent = null)
            : base(image, level, childIndex, parent) { }

        private RenderTexture depthTex2D;
        private RenderTexture normalTex2D;
        private static ComputeShader volumeShader;
        private static int volumeKernel;
        private static int numThreadsX;
        private static int numThreadsY;
        private static int numThreadsZ;

        private Thread processorThread;

        private static ComputeBuffer bytecodeMemoryBuffer;
        private static ComputeBuffer bytecodeOperationsBuffer;

        private IFuncFacade funcFacade;
        private IMassFacade massFacade;

        #region Creation

        protected override void InitConcrete()
        {
            if (level == 0)
            {
                funcFacade = ServiceLocator.Instance.GetService<IFuncFacade>();
                massFacade = ServiceLocator.Instance.GetService<IMassFacade>();

                volumeShader = Resources.Load("Shaders/Volume/VolumeShader") as ComputeShader;
                volumeKernel = volumeShader.FindKernel("RayPlot");
                volumeShader.GetKernelThreadGroupSizes(volumeKernel, out var nx, out var ny, out var nz);
                numThreadsX = (int)nx;
                numThreadsY = (int)ny;
                numThreadsZ = (int)nz;

                bytecodeMemoryBuffer = new ComputeBuffer(funcFacade.GetMaxMemorySize(), sizeof(float));
                bytecodeOperationsBuffer = new ComputeBuffer(funcFacade.GetMaxOperationsSize(), sizeof(int));

                viewFacade.onChanged += PrepareCameraInfo;
                //viewFacade.onPropertyChanged += PrepareCameraInfo;
                funcFacade.onChanged += PrepareFunctionInfo;
                regionFacade.onChanged += PrepareRegionInfo;
                massFacade.onChanged += PrepareInterpretationInfo;

                PrepareInterpretationInfo();
                PrepareCameraInfo();
                PrepareRegionInfo();
                PrepareExplorationInfo();
                PrepareFunctionInfo();
            }
            CreateTextures();
        }

        private void CreateTextures()
        {
            depthTex2D = CreateTex("depth");
            depthTex = depthTex2D;

            normalTex2D = CreateTex("normal");
            normalTex = normalTex2D;
        }

        private RenderTexture CreateTex(string name)
        {
            var tex = new RenderTexture(QuadInfo.resolution, QuadInfo.resolution, 24);
            tex.name = name;
            tex.enableRandomWrite = true;
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Create();
            return tex;
        }

        #endregion

        #region Rendering

        protected override void MemoryToTexConcrete()
        {
            
        }

        protected override void RenderInstant()
        {
            RenderRegion();
            DispatchVolumeShader();
            IsDone = true;
            IsRendering = false;
            MemoryToTex();
        }

        protected override void RenderMultiThread(int priority)
        {
            SemaphoreSlim doneSemaphore = new SemaphoreSlim(0);
            RenderingFacade.Instance.renderOrders.Add(new KeyValuePair<Action, int>(() =>
            {
                IsQueued = false;
                DispatchVolumeShader();
                doneSemaphore.Release();
            }, priority));

            processorThread = new Thread(() =>
            {
                RenderRegion();
                lock (RenderingFacade.Instance.liveThreadsLock)
                {
                    RenderingFacade.Instance.liveThreads.Remove(Thread.CurrentThread);
                }
                try
                {
                    doneSemaphore.Wait();
                    IsDone = true;
                }
                catch (ThreadAbortException)
                {
                    IsDone = false;
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
                DispatchVolumeShader();
                RenderRegion();
                IsDone = true;
            }, priority));
        }

        private void DispatchVolumeShader()
        {
            GetCameraInfo(out var right, out var up, out var nearSize, out var farSize, out var nearStart, out var farStart);

            volumeShader.SetFloats("nearStart", new float[] { nearStart.x, nearStart.y, nearStart.z });
            volumeShader.SetFloats("farStart", new float[] { farStart.x, farStart.y, farStart.z });

            volumeShader.SetFloat("width", (float)QuadInfo.resolution);
            volumeShader.SetFloat("height", (float)QuadInfo.resolution);
            volumeShader.SetFloat("region", region);
            volumeShader.SetInt("depth", QuadInfo.depthSamples);

            volumeShader.SetTexture(volumeKernel, "DepthTex", depthTex2D);
            volumeShader.SetTexture(volumeKernel, "NormalTex", normalTex2D);
            volumeShader.Dispatch(volumeKernel, QuadInfo.resolution / (int)numThreadsX, QuadInfo.resolution / (int)numThreadsY, (int)numThreadsZ);
        }

        #endregion

        #region ShaderPreparation

        private void PrepareExplorationInfo()
        {
            volumeShader.SetFloat("depthExplorationMult", RenderConfig.depthExplorationMultiplier);
            volumeShader.SetFloat("normalPlaneMultiplier", RenderConfig.normalPlaneMultiplier);
            volumeShader.SetFloat("normalExplorationMultiplier", RenderConfig.normalExplorationMultiplier);
            volumeShader.SetInt("explorationSamples", RenderConfig.explorationSamples);
        }

        private void PrepareCameraInfo()
        {
            GetCameraInfo(out var right, out var up, out var nearSize, out var farSize, out var nearStart, out var farStart);
            volumeShader.SetFloats("camRight", new float[] { right.x, right.y, right.z });
            volumeShader.SetFloats("camUp", new float[] { up.x, up.y, up.z });
            volumeShader.SetFloat("nearSize", nearSize);
            volumeShader.SetFloat("farSize", farSize);
        }

        private void PrepareRegionInfo()
        {
            volumeShader.SetBool("clampToRegion", regionFacade.IsRegionClamped());
            volumeShader.SetFloats("regionX", new float[] { regionFacade.GetRegionX().x, regionFacade.GetRegionX().y });
            volumeShader.SetFloats("regionY", new float[] { regionFacade.GetRegionY().x, regionFacade.GetRegionY().y });
            volumeShader.SetFloats("regionZ", new float[] { regionFacade.GetRegionZ().x, regionFacade.GetRegionZ().y });

            Vector3 regionScale = regionFacade.GetRegionScale();
            Vector3 regionCenter = regionFacade.GetRegionCenter();
            volumeShader.SetFloats("regionScale", new float[] { regionScale.x, regionScale.y, regionScale.z });
            volumeShader.SetFloats("regionCenter", new float[] { regionCenter.x, regionCenter.y, regionCenter.z });
        }

        private void PrepareInterpretationInfo()
        {
            int variable = massFacade.Variable;
            int criterion = massFacade.Criterion;
            volumeShader.SetInt("variable", variable);
            volumeShader.SetInt("criterion", criterion);
            volumeShader.SetFloat("difference", massFacade.MinDifference);
            volumeShader.SetFloat("threshold", massFacade.Threshold);
        }

        private void PrepareFunctionInfo()
        {
            var gpuBytecodeMemory = funcFacade.GetBytecodeMemCopy();
            int[] operations = funcFacade.GetBytecodeOperations().ToArray();
            bytecodeMemoryBuffer.SetData(gpuBytecodeMemory);
            bytecodeOperationsBuffer.SetData(operations);
            volumeShader.SetBuffer(volumeKernel, "memoryBuffer", bytecodeMemoryBuffer);
            volumeShader.SetBuffer(volumeKernel, "operationsBuffer", bytecodeOperationsBuffer);
            volumeShader.SetInt("operationsSize", operations.Length);
            volumeShader.SetInt("maxOperatorIndex", funcFacade.GetMaxOperatorIndex());
            volumeShader.SetInt("maxMemoryIndex", funcFacade.GetBytecodeMaxMemoryIndex());
            volumeShader.SetInt("resultIndex", funcFacade.GetBytecodeResultIndex());
        }

        #endregion

        #region RenderRegion

        private void RenderRegion()
        {
            GetCameraInfo(out var right, out var up, out var nearSize, out var farSize, out var nearStart, out var farStart);

            Ray ray = new Ray(RenderConfig.gpuHomogeneityDepth);

            for (int r = 0; r < 4; r++)
            {
                for (int i = 0; i < RenderConfig.homogeneityPoints; i++)
                {
                    int x = homogeneityCoords[r, i * 2];
                    int y = homogeneityCoords[r, i * 2 + 1];
                    Vector3 nearPos = nearStart + (right * ((float)x / QuadInfo.resolution) - up * ((float)y / QuadInfo.resolution)) * region * nearSize;
                    Vector3 farPos = farStart + (right * ((float)x / QuadInfo.resolution) - up * ((float)y / QuadInfo.resolution)) * region * farSize;

                    nearPos = regionFacade.TransformToRegion(ref nearPos);
                    farPos = regionFacade.TransformToRegion(ref farPos);

                    ray.SetOriginAndDestiny(ref nearPos, ref farPos);
                    float normDepth = ray.CastDepth(out var landed);
                    depths[x, y] = normDepth;
                }
            }
        }

        #endregion

        #region Destroy

        protected override void Finish()
        {
            if (level == 0)
            {
                bytecodeMemoryBuffer.Release();
                bytecodeOperationsBuffer.Release();
            }
        }

        #endregion
    }
}