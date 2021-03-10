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

    float[] bytecodeMemory;

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
            RendererManager.displayOrders.Enqueue(() =>
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
        bytecodeMemory = func.GetBytecodeMemoryArr();

        //Color farColor = Color.white;
        //Color nearColor = Color.black;

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
                    landed = func.IsMass(ref pos, bytecodeMemory);
                }
                
                Color normalColor = Color.white;
                bool reachedSurface = false;
                if (landed)
                {
                    Vector3 rayDir = (nearPos - farPos);
                    float rayDirMag = rayDir.magnitude;
                    float rayStep = rayDirMag / depth;
                    rayDir.Normalize();

                    
                    Vector3 surface = ExploreDirectionDAC(pos, pos + rayDir * rayStep * depthExplorationMultiplier, func, true, out reachedSurface);

                    normDepth = Vector3.Distance(nearPos, surface) / rayDirMag;
                    
                    Vector3 normal = CalculateNormal(surface, rayDir, rayStep, func) * 0.5f;
                    normalColor = new Color(normal.x+0.5f, normal.y+0.5f, normal.z+0.5f, 1f);
                }
                depths[x, y] = landed ? normDepth : 2f;
                int row = height - y - 1;
                depthMemory[x + row * height] = new Color(Mathf.Lerp(0f, 1f, normDepth), landed ? 1f : 0f, 0f,1f); //Color.Lerp(Color.black, Color.white, normDepth);
                normalMemory[x + row * height] = normalColor;//new Color(normal.x/*+0.5f*/, /*normal.y+0.5f*/0f, /*normal.z+0.5f*/0f, 1f);
            }
        }
        done = true;
    }

    private Vector3 CalculateNormal(Vector3 pos, Vector3 up, float explorationRadius, Function func)
    {
        Vector3 n = Vector3.zero;

        Vector3 right = new Vector3(1, 1, (-up.x - up.y) / up.z);
        right.Normalize();
        Vector3 forward = Vector3.Cross(up, right);
        forward.Normalize();

        Vector3[] points = new Vector3[]
        {
            pos+right*explorationRadius*normalPlaneMultiplier,
            pos+forward*explorationRadius*normalPlaneMultiplier,
            pos-right*explorationRadius*normalPlaneMultiplier,
            pos-forward*explorationRadius*normalPlaneMultiplier,
        };

        //Vector3[] dirs = new Vector3[points.Length];

        up = up.normalized * explorationRadius * normalExplorationMultiplier;

        for(int i = 0; i < points.Length; i++)
        {
            bool pointInside = func.IsMass(ref points[i], bytecodeMemory);
            bool reachedSurface;
            Vector3 s = ExploreDirectionDAC(points[i], points[i] + (pointInside ? up : -up), func, pointInside, out reachedSurface);
            points[i] = (s - pos).normalized;


            //dirs[i] = (points[i] - pos).normalized;
        }

        for(int i = 0; i < points.Length - 1; i++)
        {
            n += Vector3.Cross(points[0], points[1]);
        }
        n.Normalize();


        return n;
    }

    private Vector3 ExploreDirectionDAC(Vector3 origin, Vector3 destiny, Function func, bool originInside, out bool reachedSurface)
    {
        Vector3 middle = destiny;

        reachedSurface = func.IsMass(ref destiny, bytecodeMemory) != originInside;
        if (reachedSurface)
        {
            for (int i = 0; i < explorationSamples; i++)
            {
                middle = Vector3.Lerp(origin, destiny, 0.5f);
                if (func.IsMass(ref middle, bytecodeMemory) == originInside)
                {
                    origin = middle;
                }
                else
                {
                    destiny = middle;
                }
            }
        }
        
        return middle;
    }

    /*private Vector3 ExploreDirectionLinear(Vector3 origin, Vector3 destiny, Function func, out bool reachedSurface)
    {
        Vector3 pos = Vector3.zero;
        reachedSurface = false;
        Vector3 lastPos = pos;
        for (float i = 0f; i < explorationSamples; i++)
        {
            lastPos = pos;
            pos = Vector3.Lerp(origin, destiny, i / explorationSamples);
            if(!IsMass(ref pos, func))
            {
                reachedSurface = true;
                return lastPos;
            }
        }
        return destiny;
    }*/

    #endregion
}
