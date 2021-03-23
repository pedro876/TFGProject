using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
//using System.Collections.Concurrent;

public class GPURendererC : AbstractRenderer
{
    private class Coord
    {
        public int x;
        public int y;

        public Coord(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public void SetXY(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    RenderTexture depthTex2D;
    RenderTexture normalTex2D;
    static ComputeShader volumeShader;
    static int volumeKernel;
    static int numThreadsX;
    static int numThreadsY;
    static int numThreadsZ;

    Thread processorThread;
    float[,] depths;

    Coord[,] homogeneityCoords;

    float[] bytecodeMemory;

    public static int homogeneityDepth = 128;

    static ComputeBuffer bytecodeMemoryBuffer;
    static ComputeBuffer bytecodeOperationsBuffer;

    //private const int levelThreadsSleepMs = 50;

    public override void Init(int level = 0, AbstractRenderer parent = null, float startX = 0, float startY = 1, float region = 1)
    {
        if (level == 0)
        {
            volumeShader = Resources.Load("Shaders/Volume/VolumeShader") as ComputeShader;
            volumeKernel = volumeShader.FindKernel("RayPlot");
            volumeShader.GetKernelThreadGroupSizes(volumeKernel, out var nx, out var ny, out var nz);
            numThreadsX = (int)nx;
            numThreadsY = (int)ny;
            numThreadsZ = (int)nz;

            bytecodeMemoryBuffer = new ComputeBuffer(FunctionC.maxMemorySize, sizeof(float));
            bytecodeOperationsBuffer = new ComputeBuffer(FunctionC.maxOperationsSize, sizeof(int));

            ViewControllerC.onPreChanged += PrepareCameraInfo;
            ViewControllerC.onPreChanged += PrepareRegionInfo;
            FunctionMenu.onPreChanged += PrepareFunctionInfo;
            VolumeInterpreter.onPreChanged += PrepareInterpretationInfo;
            PrepareInterpretationInfo();
            PrepareCameraInfo();
            PrepareRegionInfo();
            PrepareExplorationInfo();
            if (FunctionElement.HasValidFunc) PrepareFunctionInfo();
        }

        homogeneityCoords = new Coord[4, homogeneityPoints];
        for(int r = 0; r < 4; r++)
        {
            for(int i = 0; i < homogeneityPoints; i++)
            {
                homogeneityCoords[r, i] = new Coord();
            }
        }

        base.Init(level, parent, startX, startY, region);
    }

    public override void Finish()
    {
        if(level == 0)
        {
            bytecodeMemoryBuffer.Release();
            bytecodeOperationsBuffer.Release();
        }
    }

    override protected void CreateTextures()
    {
        base.CreateTextures();

        depthTex2D = new RenderTexture(width, height, 24);
        depthTex2D.name = "depth";
        depthTex2D.enableRandomWrite = true;
        depthTex2D.filterMode = FilterMode.Bilinear;
        depthTex2D.wrapMode = TextureWrapMode.Clamp;
        depthTex2D.Create();
        depthTex = depthTex2D;

        normalTex2D = new RenderTexture(width, height, 24);
        normalTex2D.name = "normal";
        normalTex2D.enableRandomWrite = true;
        normalTex2D.filterMode = FilterMode.Bilinear;
        normalTex2D.wrapMode = TextureWrapMode.Clamp;
        normalTex2D.Create();
        normalTex = normalTex2D;

        depths = new float[width, height];
    }

    protected override void GetRandomDepths(int r, float[] homogeneityDepths)
    {

        for (int i = 0; i < homogeneityPoints; i++)
        {
            Coord c = homogeneityCoords[r, i];
            homogeneityDepths[i] = depths[c.x, c.y];
        }
    }

    #region rendering

    public override void Render()
    {
        base.Render();
        if (done) return;
        CalculateHomogeneityPoints();
        if (level == 0)
        {
            RenderRegion();
            DispatchVolumeShader();
            done = true;
            rendering = false;
            MemoryToTex();
        } else
        {
            queued = true;
            int priority = Mathf.RoundToInt(GetImportance() * 100);
            if(RendererManager.Instance.maxParallelThreads > 0)
            {
                SemaphoreSlim doneSemaphore = new SemaphoreSlim(0);
                RendererManager.renderOrders.Add(new KeyValuePair<System.Action, int>(() =>
                {
                    queued = false;
                    DispatchVolumeShader();
                    doneSemaphore.Release();
                }, priority));

                processorThread = new Thread(() =>
                {
                    RenderRegion();

                    lock (RendererManager.currentThreadsLock)
                    {
                        RendererManager.currentThreads.Remove(Thread.CurrentThread);
                    }
                    try
                    {
                        doneSemaphore.Wait();
                        done = true;
                    }
                    catch (ThreadAbortException)
                    {
                        done = false;
                    }
                }
                );
                RendererManager.threadsToStart.Add(new KeyValuePair<Thread, int>(processorThread, priority));
            } else
            {
                RendererManager.renderOrders.Add(new KeyValuePair<System.Action, int>(() =>
                {
                    queued = false;
                    DispatchVolumeShader();
                    RenderRegion();
                    done = true;
                }, priority));
            }
            
        }
    }

    
    private void PrepareExplorationInfo()
    {
        volumeShader.SetFloat("depthExplorationMult", depthExplorationMultiplier);
        volumeShader.SetFloat("normalPlaneMultiplier", normalPlaneMultiplier);
        volumeShader.SetFloat("normalExplorationMultiplier", normalExplorationMultiplier);
        volumeShader.SetInt("explorationSamples", explorationSamples);
    }

    private void PrepareCameraInfo()
    {
        //Debug.Log("Preparing camera info");
        Vector3 right = (ViewControllerC.NearTopRight - ViewControllerC.NearTopLeft).normalized;
        Vector3 up = (ViewControllerC.NearTopLeft - ViewControllerC.NearBottomLeft).normalized;
        float nearSize = Vector3.Distance(ViewControllerC.NearTopLeft, ViewControllerC.NearTopRight);
        float farSize = Vector3.Distance(ViewControllerC.FarTopLeft, ViewControllerC.FarTopRight);
        volumeShader.SetFloats("camRight", new float[] { right.x, right.y, right.z });
        volumeShader.SetFloats("camUp", new float[] { up.x, up.y, up.z });
        volumeShader.SetFloat("nearSize", nearSize);
        volumeShader.SetFloat("farSize", farSize);
    }

    private void PrepareRegionInfo()
    {
        volumeShader.SetBool("clampToRegion", ViewControllerC.GetClampToRegion());
        volumeShader.SetFloats("regionX", new float[] { ViewControllerC.regionX.x, ViewControllerC.regionX.y });
        volumeShader.SetFloats("regionY", new float[] { ViewControllerC.regionY.x, ViewControllerC.regionY.y });
        volumeShader.SetFloats("regionZ", new float[] { ViewControllerC.regionZ.x, ViewControllerC.regionZ.y });

        Vector3 regionScale = ViewControllerC.GetRegionScale();
        Vector3 regionCenter = ViewControllerC.GetRegionCenter();
        volumeShader.SetFloats("regionScale", new float[] { regionScale.x, regionScale.y, regionScale.z });
        volumeShader.SetFloats("regionCenter", new float[] { regionCenter.x, regionCenter.y, regionCenter.z });
    }

    private void PrepareInterpretationInfo()
    {
        int variable = (int)VolumeInterpreter.Variable;
        int criterion = (int)VolumeInterpreter.Criterion;
        volumeShader.SetInt("variable", variable);
        volumeShader.SetInt("criterion", criterion);
        volumeShader.SetFloat("difference", VolumeInterpreter.MinDifference);
        volumeShader.SetFloat("threshold", VolumeInterpreter.Threshold);
    }

    static bool functionInfoPrepared = false;
    private void PrepareFunctionInfo()
    {
        //Debug.Log("Preparing function info");
        functionInfoPrepared = true;
        if (!FunctionElement.HasValidFunc) return;
        FunctionC func = FunctionElement.selectedFunc.func;
        float[] gpuBytecodeMemory = func.CreateBytecodeMemory();
        int[] operations = func.GetBytecode();
        bytecodeMemoryBuffer.SetData(gpuBytecodeMemory);
        bytecodeOperationsBuffer.SetData(operations);
        volumeShader.SetBuffer(volumeKernel, "memoryBuffer", bytecodeMemoryBuffer);
        volumeShader.SetBuffer(volumeKernel, "operationsBuffer", bytecodeOperationsBuffer);
        volumeShader.SetInt("operationsSize", operations.Length);
        volumeShader.SetInt("maxOperatorIndex", FunctionNode.GetMaxOperatorIndex);
        volumeShader.SetInt("maxMemoryIndex", gpuBytecodeMemory.Length);
        volumeShader.SetInt("resultIndex", func.resultIndex);
    }

    private void DispatchVolumeShader()
    {
        if (!functionInfoPrepared && level == 0)
        {
            if (FunctionElement.selectedFunc != null && FunctionElement.selectedFunc.func != null)
            {
                PrepareFunctionInfo();
            }
            else return;
        }

        Vector3 up = (ViewControllerC.NearTopLeft - ViewControllerC.NearBottomLeft).normalized;
        float nearSize = Vector3.Distance(ViewControllerC.NearTopLeft, ViewControllerC.NearTopRight);
        float farSize = Vector3.Distance(ViewControllerC.FarTopLeft, ViewControllerC.FarTopRight);
        Vector3 nearStart = Vector3.Lerp(ViewControllerC.NearTopLeft, ViewControllerC.NearTopRight, startX) - up * (1f - startY) * nearSize;
        Vector3 farStart = Vector3.Lerp(ViewControllerC.FarTopLeft, ViewControllerC.FarTopRight, startX) - up * (1f - startY) * farSize;

        /*if(ViewController.changed && level == 0)
        {
            PrepareCameraInfo();
            PrepareRegionInfo();
        }*/


        volumeShader.SetFloats("nearStart", new float[] { nearStart.x, nearStart.y, nearStart.z });
        volumeShader.SetFloats("farStart", new float[] { farStart.x, farStart.y, farStart.z });

        volumeShader.SetFloat("width", (float)width);
        volumeShader.SetFloat("height", (float)height);
        volumeShader.SetFloat("region", region);
        volumeShader.SetInt("depth", depth);

        volumeShader.SetTexture(volumeKernel, "DepthTex", depthTex2D);
        volumeShader.SetTexture(volumeKernel, "NormalTex", normalTex2D);
        volumeShader.Dispatch(volumeKernel, width / (int)numThreadsX, height / (int)numThreadsY, (int)numThreadsZ);
    }

    private void CalculateHomogeneityPoints()
    {
        for (int r = 0; r < 4; r++)
        {
            int minX, minY, maxX, maxY;
            GetSubRegion(r, out minX, out minY, out maxX, out maxY);
            homogeneityCoords[r,0].SetXY(minX, minY);
            homogeneityCoords[r,1].SetXY(minX, maxY - 1);
            homogeneityCoords[r,2].SetXY(maxX - 1, minY);
            homogeneityCoords[r,3].SetXY(maxX - 1, maxY - 1);

            for (int j = 4; j < homogeneityPoints; j++)
            {
                homogeneityCoords[r,j].SetXY(Random.Range(minX, maxX), Random.Range(minY, maxY));
            }
        }
    }

    private void RenderRegion()
    {
        Vector3 right = (ViewControllerC.NearTopRight - ViewControllerC.NearTopLeft).normalized;
        Vector3 up = (ViewControllerC.NearTopLeft - ViewControllerC.NearBottomLeft).normalized;

        float nearSize = Vector3.Distance(ViewControllerC.NearTopLeft, ViewControllerC.NearTopRight);
        float farSize = Vector3.Distance(ViewControllerC.FarTopLeft, ViewControllerC.FarTopRight);
        Vector3 nearStart = Vector3.Lerp(ViewControllerC.NearTopLeft, ViewControllerC.NearTopRight, startX) - up * (1f - startY) * nearSize;
        Vector3 farStart = Vector3.Lerp(ViewControllerC.FarTopLeft, ViewControllerC.FarTopRight, startX) - up * (1f - startY) * farSize;
        FunctionC func = FunctionElement.selectedFunc.func;
        bytecodeMemory = func.CreateBytecodeMemory();
        for(int r = 0; r < 4; r++)
        {
            for (int i = 0; i < homogeneityPoints; i++)
            {
                int x = homogeneityCoords[r,i].x;
                int y = homogeneityCoords[r,i].y;
                Vector3 nearPos = nearStart + (right * ((float)x / width) - up * ((float)y / height)) * region * nearSize;
                Vector3 farPos = farStart + (right * ((float)x / width) - up * ((float)y / height)) * region * farSize;

                nearPos = ViewControllerC.TransformToRegion(ref nearPos);
                farPos = ViewControllerC.TransformToRegion(ref farPos);

                bool landed = false;
                float normDepth = 0f;
                Vector3 pos = Vector3.zero;
                for (int z = 0; z < homogeneityDepth && !landed; z++)
                {
                    normDepth = (float)z / (homogeneityDepth - 1);
                    pos = Vector3.Lerp(nearPos, farPos, normDepth);
                    landed = func.IsMass(ref pos, bytecodeMemory);
                }
                depths[x, y] = landed ? normDepth : 2f;
            }
        }
    }

    #endregion
}
