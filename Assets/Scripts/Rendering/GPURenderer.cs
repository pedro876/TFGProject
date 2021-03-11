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

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    RenderTexture depthTex2D;
    RenderTexture normalTex2D;
    ComputeShader volumeShader;
    int volumeKernel;
    uint numThreadsX;
    uint numThreadsY;
    uint numThreadsZ;

    Thread processorThread;
    float[,] depths;

    List<Coord[]> homogeneityCoords;

    float[] bytecodeMemory;

    public static int homogeneityDepth = 128;

    ComputeBuffer bytecodeMemoryBuffer;
    ComputeBuffer bytecodeOperationsBuffer;

    //private const int levelThreadsSleepMs = 50;

    protected override float[] GetRandomDepths(int r)
    {
        float[] homogeneityDepths = new float[homogeneityPoints];

        for(int i = 0; i < homogeneityPoints; i++)
        {
            Coord c = homogeneityCoords[r][i];
            homogeneityDepths[i] = depths[c.x, c.y];
        }

        return homogeneityDepths;
    }

    public override void Init(int level = 0, Renderer parent = null, float startX = 0, float startY = 1, float region = 1)
    {
        volumeShader = Resources.Load("Shaders/VolumeShader") as ComputeShader;
        volumeKernel = volumeShader.FindKernel("CSMain");
        volumeShader.GetKernelThreadGroupSizes(volumeKernel, out numThreadsX, out numThreadsY, out numThreadsZ);

        bytecodeMemoryBuffer = new ComputeBuffer(Function.maxMemorySize, sizeof(float));
        bytecodeOperationsBuffer = new ComputeBuffer(Function.maxOperationsSize, sizeof(int));

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
            //RenderRegion(0, 0, width, height);
            DispatchVolumeShader();
            RenderRegion(homogeneityCoords);
            done = true;
            rendering = false;
            RendererManager.displayOrders.Enqueue(() =>
            {
                MemoryToTex();
                RenderChildren();
            });
        } else
        {
            queued = true;
            int priority = Mathf.RoundToInt(GetImportance() * 100);
            SemaphoreSlim doneSemaphore = new SemaphoreSlim(0);
            //RenderRegion(homogeneityCoords);
            RendererManager.renderOrders.Add(new KeyValuePair<System.Action, int>(()=>
            {
                queued = false;
                DispatchVolumeShader();
                //done = true;
                doneSemaphore.Release();
            } ,priority));
            
            processorThread = new Thread(() =>
            {
                RenderRegion(homogeneityCoords);
                
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

    private void DispatchVolumeShader()
    {
        Vector3 right = (ViewController.nearTopRight - ViewController.nearTopLeft).normalized;
        Vector3 up = (ViewController.nearTopLeft - ViewController.nearBottomLeft).normalized;
        float nearSize = Vector3.Distance(ViewController.nearTopLeft, ViewController.nearTopRight);
        float farSize = Vector3.Distance(ViewController.farTopLeft, ViewController.farTopRight);
        Vector3 nearStart = Vector3.Lerp(ViewController.nearTopLeft, ViewController.nearTopRight, startX) - up * (1f - startY) * nearSize;
        Vector3 farStart = Vector3.Lerp(ViewController.farTopLeft, ViewController.farTopRight, startX) - up * (1f - startY) * farSize;

        volumeShader.SetFloats("right", new float[] { right.x, right.y, right.z });
        volumeShader.SetFloats("up", new float[] { up.x, up.y, up.z });
        volumeShader.SetFloat("nearSize", nearSize);
        volumeShader.SetFloat("farSize", farSize);
        volumeShader.SetFloats("nearStart", new float[] { nearStart.x, nearStart.y, nearStart.z });
        volumeShader.SetFloats("farStart", new float[] { farStart.x, farStart.y, farStart.z });

        volumeShader.SetFloat("width", (float)width);
        volumeShader.SetFloat("height", (float)height);
        volumeShader.SetFloat("region", region);
        volumeShader.SetFloat("depth", (float)depth);

        volumeShader.SetFloat("depthExplorationMult", depthExplorationMultiplier);
        volumeShader.SetFloat("normalPlaneMultiplier", normalPlaneMultiplier);
        volumeShader.SetFloat("normalExplorationMultiplier", normalExplorationMultiplier);

        volumeShader.SetBool("clampToRegion", ViewController.GetClampToRegion());
        volumeShader.SetFloats("regionX", new float[] { ViewController.regionX.x, ViewController.regionX.y });
        volumeShader.SetFloats("regionY", new float[] { ViewController.regionY.x, ViewController.regionY.y });
        volumeShader.SetFloats("regionZ", new float[] { ViewController.regionZ.x, ViewController.regionZ.y });

        Vector3 regionScale = ViewController.GetRegionScale();
        Vector3 regionCenter = ViewController.GetRegionCenter();
        volumeShader.SetFloats("regionScale", new float[] { regionScale.x, regionScale.y, regionScale.z });
        volumeShader.SetFloats("regionCenter", new float[] { regionCenter.x, regionCenter.y, regionCenter.z });

        volumeShader.SetInt("variable", (int)VolumeInterpreter.variable);
        volumeShader.SetInt("criterion", (int)VolumeInterpreter.criterion);
        volumeShader.SetFloat("threshold", VolumeInterpreter.threshold);

        volumeShader.SetInt("explorationSamples", explorationSamples);

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

        volumeShader.SetTexture(volumeKernel, "DepthTex", depthTex2D);
        volumeShader.SetTexture(volumeKernel, "NormalTex", normalTex2D);
        volumeShader.Dispatch(volumeKernel, width / (int)numThreadsX, height / (int)numThreadsY, (int)numThreadsZ);
    }

    private void CalculateHomogeneityPoints()
    {
        homogeneityCoords = new List<Coord[]>();
        for (int i = 0; i < 4; i++)
        {
            Coord[] c = new Coord[homogeneityPoints];
            int minX, minY, maxX, maxY;
            GetSubRegion(i, out minX, out minY, out maxX, out maxY);
            c[0] = new Coord(minX, minY);
            c[1] = new Coord(minX, maxY - 1);
            c[2] = new Coord(maxX - 1, minY);
            c[3] = new Coord(maxX - 1, maxY - 1);

            for (int j = 4; j < homogeneityPoints; j++)
            {
                c[j] = new Coord(Random.Range(minX, maxX), Random.Range(minY, maxY));
            }
            homogeneityCoords.Add(c);
        }
    }

    private void RenderRegion(List<Coord[]> coordsList)
    {
        
        Vector3 right = (ViewController.nearTopRight - ViewController.nearTopLeft).normalized;
        Vector3 up = (ViewController.nearTopLeft - ViewController.nearBottomLeft).normalized;

        float nearSize = Vector3.Distance(ViewController.nearTopLeft, ViewController.nearTopRight);
        float farSize = Vector3.Distance(ViewController.farTopLeft, ViewController.farTopRight);
        Vector3 nearStart = Vector3.Lerp(ViewController.nearTopLeft, ViewController.nearTopRight, startX) - up * (1f - startY) * nearSize;
        Vector3 farStart = Vector3.Lerp(ViewController.farTopLeft, ViewController.farTopRight, startX) - up * (1f - startY) * farSize;
        Function func = FunctionElement.selectedFunc.func;
        bytecodeMemory = func.GetBytecodeMemoryArr();

        foreach(var coords in coordsList)
        {
            for (int i = 0; i < homogeneityPoints; i++)
            {
                int x = coords[i].x;
                int y = coords[i].y;
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
