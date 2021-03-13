using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
//using System.Collections.Concurrent;

public class GPURenderer : Renderer
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
        public void Set(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    RenderTexture depthTex2D;
    RenderTexture normalTex2D;
    static ComputeShader volumeShader;
    static int volumeKernel;
    static uint numThreadsX;
    static uint numThreadsY;
    static uint numThreadsZ;

    Thread processorThread;
    float[,] depths;

    Coord[,] homogeneityCoords;

    float[] bytecodeMemory;

    public static int homogeneityDepth = 128;

    static ComputeBuffer bytecodeMemoryBuffer;
    static ComputeBuffer bytecodeOperationsBuffer;

    //private const int levelThreadsSleepMs = 50;

    public override void Init(int level = 0, Renderer parent = null, float startX = 0, float startY = 1, float region = 1)
    {
        if (level == 0)
        {
            volumeShader = Resources.Load("Shaders/VolumeShader") as ComputeShader;
            volumeKernel = volumeShader.FindKernel("CSMain");
            volumeShader.GetKernelThreadGroupSizes(volumeKernel, out numThreadsX, out numThreadsY, out numThreadsZ);

            bytecodeMemoryBuffer = new ComputeBuffer(Function.maxMemorySize, sizeof(float));
            bytecodeOperationsBuffer = new ComputeBuffer(Function.maxOperationsSize, sizeof(int));

            //ViewController.onPreChanged += PrepareCameraInfo;
            //ViewController.onPreChanged += PrepareRegionInfo;
            FunctionPanel.onPreChanged += PrepareFunctionInfo;
            VolumeInterpreter.onPreChanged += PrepareInterpretationInfo;
            PrepareInterpretationInfo();
            PrepareCameraInfo();
            PrepareRegionInfo();
            PrepareExplorationInfo();
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
        if(FunctionElement.selectedFunc == null || FunctionElement.selectedFunc.func == null)
        {
            done = true;
            rendering = false;
        }
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
            SemaphoreSlim doneSemaphore = new SemaphoreSlim(0);
            RendererManager.renderOrders.Add(new KeyValuePair<System.Action, int>(()=>
            {
                queued = false;
                DispatchVolumeShader();
                doneSemaphore.Release();
            } ,priority));
            
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
                } catch (ThreadAbortException e)
                {
                    done = false;
                }
            }
            );
            RendererManager.threadsToStart.Add(new KeyValuePair<Thread, int>(processorThread, priority));
        }
    }

    
    private void PrepareExplorationInfo()
    {
        //Debug.Log("Preparing exploration info");
        volumeShader.SetFloat("depthExplorationMult", depthExplorationMultiplier);
        volumeShader.SetFloat("normalPlaneMultiplier", normalPlaneMultiplier);
        volumeShader.SetFloat("normalExplorationMultiplier", normalExplorationMultiplier);
        volumeShader.SetInt("explorationSamples", explorationSamples);
    }

    private void PrepareCameraInfo()
    {
        //Debug.Log("Preparing camera info");
        Vector3 right = (ViewController.nearTopRight - ViewController.nearTopLeft).normalized;
        Vector3 up = (ViewController.nearTopLeft - ViewController.nearBottomLeft).normalized;
        float nearSize = Vector3.Distance(ViewController.nearTopLeft, ViewController.nearTopRight);
        float farSize = Vector3.Distance(ViewController.farTopLeft, ViewController.farTopRight);
        volumeShader.SetFloats("right", new float[] { right.x, right.y, right.z });
        volumeShader.SetFloats("up", new float[] { up.x, up.y, up.z });
        volumeShader.SetFloat("nearSize", nearSize);
        volumeShader.SetFloat("farSize", farSize);
    }

    private void PrepareRegionInfo()
    {
        //Debug.Log("Preparing region info");
        volumeShader.SetBool("clampToRegion", ViewController.GetClampToRegion());
        volumeShader.SetFloats("regionX", new float[] { ViewController.regionX.x, ViewController.regionX.y });
        volumeShader.SetFloats("regionY", new float[] { ViewController.regionY.x, ViewController.regionY.y });
        volumeShader.SetFloats("regionZ", new float[] { ViewController.regionZ.x, ViewController.regionZ.y });

        Vector3 regionScale = ViewController.GetRegionScale();
        Vector3 regionCenter = ViewController.GetRegionCenter();
        volumeShader.SetFloats("regionScale", new float[] { regionScale.x, regionScale.y, regionScale.z });
        volumeShader.SetFloats("regionCenter", new float[] { regionCenter.x, regionCenter.y, regionCenter.z });
    }

    private void PrepareInterpretationInfo()
    {
        //Debug.Log("Preparing interpretation info");
        volumeShader.SetInt("variable", (int)VolumeInterpreter.variable);
        volumeShader.SetInt("criterion", (int)VolumeInterpreter.criterion);
        volumeShader.SetFloat("threshold", VolumeInterpreter.threshold);
    }

    static bool functionInfoPrepared = false;
    private void PrepareFunctionInfo()
    {
        //Debug.Log("Preparing function info");
        functionInfoPrepared = true;
        Function func = FunctionElement.selectedFunc.func;
        float[] gpuBytecodeMemory = func.GetBytecodeMemoryArr();
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

        Vector3 up = (ViewController.nearTopLeft - ViewController.nearBottomLeft).normalized;
        float nearSize = Vector3.Distance(ViewController.nearTopLeft, ViewController.nearTopRight);
        float farSize = Vector3.Distance(ViewController.farTopLeft, ViewController.farTopRight);
        Vector3 nearStart = Vector3.Lerp(ViewController.nearTopLeft, ViewController.nearTopRight, startX) - up * (1f - startY) * nearSize;
        Vector3 farStart = Vector3.Lerp(ViewController.farTopLeft, ViewController.farTopRight, startX) - up * (1f - startY) * farSize;

        if(ViewController.changed && level == 0)
        {
            PrepareCameraInfo();
            PrepareRegionInfo();
        }


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
            homogeneityCoords[r,0].Set(minX, minY);
            homogeneityCoords[r,1].Set(minX, maxY - 1);
            homogeneityCoords[r,2].Set(maxX - 1, minY);
            homogeneityCoords[r,3].Set(maxX - 1, maxY - 1);

            for (int j = 4; j < homogeneityPoints; j++)
            {
                homogeneityCoords[r,j].Set(Random.Range(minX, maxX), Random.Range(minY, maxY));
            }
        }
    }

    private void RenderRegion()
    {
        
        Vector3 right = (ViewController.nearTopRight - ViewController.nearTopLeft).normalized;
        Vector3 up = (ViewController.nearTopLeft - ViewController.nearBottomLeft).normalized;

        float nearSize = Vector3.Distance(ViewController.nearTopLeft, ViewController.nearTopRight);
        float farSize = Vector3.Distance(ViewController.farTopLeft, ViewController.farTopRight);
        Vector3 nearStart = Vector3.Lerp(ViewController.nearTopLeft, ViewController.nearTopRight, startX) - up * (1f - startY) * nearSize;
        Vector3 farStart = Vector3.Lerp(ViewController.farTopLeft, ViewController.farTopRight, startX) - up * (1f - startY) * farSize;
        Function func = FunctionElement.selectedFunc.func;
        bytecodeMemory = func.GetBytecodeMemoryArr();
        for(int r = 0; r < 4; r++)
        {
            for (int i = 0; i < homogeneityPoints; i++)
            {
                int x = homogeneityCoords[r,i].x;
                int y = homogeneityCoords[r,i].y;
                Vector3 nearPos = nearStart + (right * ((float)x / width) - up * ((float)y / height)) * region * nearSize;
                Vector3 farPos = farStart + (right * ((float)x / width) - up * ((float)y / height)) * region * farSize;

                nearPos = ViewController.TransformToRegion(ref nearPos);
                farPos = ViewController.TransformToRegion(ref farPos);

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
