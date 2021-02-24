using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class CPURenderer : Renderer
{
    Texture2D renderTex;
    Thread processorThread;

    private const int levelThreadsSleepMs = 50;

    override protected void CreateTexture()
    {
        base.CreateTexture();
        renderTex = new Texture2D(width, height);
        renderTex.filterMode = FilterMode.Trilinear;
        renderTex.wrapMode = TextureWrapMode.Clamp;
        tex = renderTex;
    }

    public override void DeepStop()
    {
        if (processorThread != null && processorThread.IsAlive) processorThread.Abort();
        base.DeepStop();
    }

    protected override void MemoryToTex()
    {
        renderTex.SetPixels(memory);
        renderTex.Apply();
        image.enabled = true;
    }

    public override void Render()
    {
        base.Render();
        if(FunctionElement.selectedFunc == null || FunctionElement.selectedFunc.func == null)
        {
            done = true;
            rendering = false;
            //Debug.Log("No func");
        }
        else if(level == 0)
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
            int time = Random.Range(10,levelThreadsSleepMs);
            processorThread = new Thread(() =>
            {
                Thread.Sleep(time);
                RenderRegion(0, 0, width, height);
            }
            );
            processorThread.Start();
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

        Color blankColor = new Color(0f, 0f, 0f, 0f);
        Color landColor = Color.blue;

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
                int row = height - y - 1;
                memory[x+ row*height] = landed ? landColor * (1f - normDepth) : blankColor;
            }
        }
        done = true;
    }
}
