using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
//using System.Collections.Concurrent;

public class CPURenderer : Renderer
{

    Texture2D depthTex2D;
    Texture2D normalTex2D;

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

    override protected void CreateTextures()
    {
        base.CreateTextures();

        depthTex2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
        depthTex2D.filterMode = FilterMode.Bilinear;
        depthTex2D.wrapMode = TextureWrapMode.Clamp;
        depthTex = depthTex2D;

        normalTex2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
        normalTex2D.filterMode = FilterMode.Bilinear;
        normalTex2D.wrapMode = TextureWrapMode.Clamp;
        normalTex = normalTex2D;

        depths = new float[width, height];
    }

    protected override void MemoryToTex()
    {
        depthTex2D.SetPixels(depthMemory);
        depthTex2D.Apply();

        normalTex2D.SetPixels(normalMemory);
        normalTex2D.Apply();
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
                Vector3 pos = Vector3.zero;
                for (int z = 0; z < depth && !landed; z++)
                {
                    normDepth = (float)z / (depth-1);
                    pos = Vector3.Lerp(nearPos, farPos, normDepth);
                    if (!IsOutOfRegion(ref pos))
                    {
                        landed = func.IsMass(ref pos);
                    }
                    
                    /*float eval = func.Solve(pos.x, pos.z, pos.y);
                    if (pos.y <= eval)
                    {
                        landed = true;
                    }*/
                }
                depths[x, y] = normDepth;
                int row = height - y - 1;
                depthMemory[x + row * height] = new Color(Mathf.Lerp(0f, 1f, normDepth), landed ? 1f : 0f, 0f, 1f); /*Color.Lerp(nearColor, farColor, normDepth);*/
                Color normalColor = Color.white;
                if (landed)
                {
                    Vector3 normal = CalculateNormal(pos, func);
                    normalColor = new Color(normal.x, normal.y, normal.z, 1f);
                    //Vector3 normal = CalculateNormal(pos, func) * 0.5f;
                    //normalColor = new Color(normal.x+0.5f, normal.y+0.5f, normal.z+0.5f, 1f);
                }


                normalMemory[x + row * height] = normalColor;//new Color(normal.x/*+0.5f*/, /*normal.y+0.5f*/0f, /*normal.z+0.5f*/0f, 1f);
            }
        }
        done = true;
    }

    private Vector3 CalculateNormal(Vector3 pos, Function func)
    {
        //Vector3 n = Vector3.zero;
        float totalMag = 0f;

        Vector3[] dirsOut = new Vector3[dirsExplored.Length];
        float[] mags = new float[dirsExplored.Length];

        for(int i = 0; i < dirsExplored.Length; i++)
        {
            Vector3 destiny = pos + dirsExplored[i] * normalExplorationRadius;
            Vector3 surface = ExploreDirectionLinear(pos, destiny, func);
            dirsOut[i] = surface - pos;
            mags[i] = dirsOut[i].magnitude;
            totalMag += mags[i];
            //n += (destiny - ExploreDirectionLinear(pos, destiny, func));
            //n += (destiny - ExploreDirectionLinear(pos, destiny, func));
        }
        Vector3 n = Vector3.zero;
        for(int i = 0; i < dirsOut.Length; i++)
        {
            n += dirsOut[i].normalized * (1f - mags[i] / totalMag);
            //dirs[i] = dirs[i].normalized * (1f - mags[i] / totalMag);
        }
        n.Normalize();

        return n;
    }

    private bool IsOutOfRegion(ref Vector3 pos)
    {
        return pos.x < -0.5f || pos.x > 0.5f || pos.y < -0.5f || pos.y > 0.5f || pos.z < -0.5f || pos.z > 0.5f;
    }

    private Vector3 ExploreDirectionDAC(Vector3 origin, Vector3 destiny, Function func)
    {

        for(int i = 0; i < normalExplorationDepth; i++)
        {
            Vector3 middle = Vector3.Lerp(origin, destiny, 0.5f);
            if(!IsOutOfRegion(ref middle) && func.IsMass(ref middle))
            {
                origin = middle;
            } else
            {
                destiny = middle;
            }
        }
        return destiny;
    }

    private Vector3 ExploreDirectionLinear(Vector3 origin, Vector3 destiny, Function func)
    {
        Vector3 pos = Vector3.zero;
        for (float i = 0f; i < normalExplorationDepth; i++)
        {
            pos = Vector3.Lerp(origin, destiny, i / normalExplorationDepth);
            if(IsOutOfRegion(ref pos) || !func.IsMass(ref pos))
            {
                return pos;
            }
        }
        return pos;
    }

    #endregion
}
