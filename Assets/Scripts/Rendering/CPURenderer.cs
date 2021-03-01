using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
//using System.Collections.Concurrent;

public class CPURenderer : Renderer
{

    Texture2D renderTex;
    Thread processorThread;
    float[,] depths;

    //private const int levelThreadsSleepMs = 50;

    protected override float[] GetRandomDepths(int minX, int minY, int maxX, int maxY)
    {
        float[] homogeneityDepths = new float[homogeneityPoints];

        homogeneityDepths[0] = depths[minX, minY];
        homogeneityDepths[1] = depths[minX, maxY-1];
        homogeneityDepths[2] = depths[maxX-1, minY];
        homogeneityDepths[3] = depths[maxX-1, maxY-1];

        for (int i = 4; i < homogeneityPoints; i++)
        {
            int x = Random.Range(minX, maxX);
            int y = Random.Range(minY, maxY);
            homogeneityDepths[i] = depths[x, y];
        }
        
        
        return homogeneityDepths;
    }

    override protected void CreateTexture()
    {
        base.CreateTexture();
        renderTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        renderTex.filterMode = FilterMode.Bilinear;
        renderTex.wrapMode = TextureWrapMode.Clamp;
        tex = renderTex;
        depths = new float[width, height];
    }

    protected override void MemoryToTex()
    {
        renderTex.SetPixels(memory);
        renderTex.Apply();
        base.MemoryToTex();
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
        if(level == 0)
        {
            RenderRegion(0, 0, width, height);
            done = true;
            rendering = false;
            RendererManager.orders.Enqueue(() =>
            {
                MemoryToTex();
                RenderChildren();
            });
        } else
        {
            queued = true;
            processorThread = new Thread(() =>
            {
                queued = false;
                RenderRegion(0, 0, width, height);
                lock (RendererManager.currentThreadsLock)
                {
                    RendererManager.currentThreads.Remove(Thread.CurrentThread);
                }
            }
            );
            int priority = Mathf.RoundToInt(GetImportance() * 100);
            //if(GetDisparity() > 0.01f) processorThread.Priority = System.Threading.ThreadPriority.AboveNormal;
            /*switch (priority / 33)
            {
                case 0: processorThread.Priority = System.Threading.ThreadPriority.Normal; break;
                case 1: processorThread.Priority = System.Threading.ThreadPriority.AboveNormal; break;
                default: processorThread.Priority = System.Threading.ThreadPriority.Highest; break;
            }*/
            RendererManager.threadsToStart.Add(new KeyValuePair<Thread, int>(processorThread, priority));
        }
    }

    private void RenderRegion(int minX, int minY, int maxX, int maxY)
    {
        Vector3 right = (ViewController.nearTopRight - ViewController.nearTopLeft).normalized;
        Vector3 up = (ViewController.nearTopLeft - ViewController.nearBottomLeft).normalized;

        float nearSize = Vector3.Distance(ViewController.nearTopLeft, ViewController.nearTopRight);
        float farSize = Vector3.Distance(ViewController.farTopLeft, ViewController.farTopRight);
        Vector3 nearStart = Vector3.Lerp(ViewController.nearTopLeft, ViewController.nearTopRight, startX) - up * (1f - startY) * nearSize;
        Vector3 farStart = Vector3.Lerp(ViewController.farTopLeft, ViewController.farTopRight, startX) - up * (1f - startY) * farSize;
        Function func = FunctionElement.selectedFunc.func;

        Color farColor = Color.white;
        Color nearColor = Color.black;

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                Vector3 nearPos = nearStart + (right * ((float)x / width) - up * ((float)y / height)) * region * nearSize;
                Vector3 farPos = farStart + (right * ((float)x / width) - up * ((float)y / height)) * region * farSize;

                bool landed = false;
                float normDepth = 0f;
                for (int z = 0; z < depth && !landed; z++)
                {
                    normDepth = (float)z / (depth-1);
                    Vector3 pos = Vector3.Lerp(nearPos, farPos, normDepth);
                    if (pos.x < -0.5f || pos.x > 0.5f || pos.y < -0.5f || pos.y > 0.5f || pos.z < -0.5f || pos.z > 0.5f) continue;
                    float eval = func.Solve(pos.x, pos.z, pos.y);
                    if (pos.y <= eval)
                    {
                        landed = true;
                    }
                }
                depths[x, y] = normDepth;
                int row = height - y - 1;
                memory[x + row * height] = Color.Lerp(nearColor, farColor, normDepth);
            }
        }
        done = true;
    }

   

    #endregion
}
