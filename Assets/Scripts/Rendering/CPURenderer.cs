using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPURenderer : Renderer
{
    Texture2D renderTex;

    override protected void CreateTexture()
    {
        base.CreateTexture();
        renderTex = new Texture2D(width, height);
        renderTex.filterMode = FilterMode.Point;
        renderTex.wrapMode = TextureWrapMode.Clamp;
        tex = renderTex;
    }

    protected override void RenderRegion()
    {
        base.RenderRegion();
        Vector3 right = (ViewController.nearTopRight - ViewController.nearTopLeft).normalized;
        Vector3 up = (ViewController.nearTopLeft - ViewController.nearBottomLeft).normalized;
        //Vector3 forward = Vector3.Cross(up, right).normalized;

        float nearSize = Vector3.Distance(ViewController.nearTopLeft, ViewController.nearTopRight);
        float farSize = Vector3.Distance(ViewController.farTopLeft, ViewController.farTopRight);
        Vector3 nearStart = Vector3.Lerp(ViewController.nearTopLeft, ViewController.nearTopRight, startX) - up * (1f-startY) * nearSize;
        Vector3 farStart = Vector3.Lerp(ViewController.farTopLeft, ViewController.farTopRight, startX) - up * (1f-startY) * farSize;
        Debug.DrawLine(nearStart, farStart, Color.blue);
        Function func = FunctionElement.selectedFunc.func;

        Color blankColor = new Color(0f, 0f, 0f, 0f);
        Color landColor = Color.blue;
        
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                Vector3 nearPos = nearStart + (right * ((float)x / width) - up * ((float)y / height)) * region*nearSize;
                Vector3 farPos = farStart + (right * ((float)x / width) - up * ((float)y / height)) * region*farSize;

                bool landed = false;
                float normDepth = 0f;
                for (int z = 0; z < depth && !landed; z++)
                {
                    normDepth = (float)z / depth;
                    Vector3 pos = Vector3.Lerp(nearPos, farPos, normDepth);
                    if (pos.x < -0.5f || pos.x > 0.5f || pos.y < -0.5f || pos.y > 0.5f || pos.z < -0.5f || pos.z > 0.5f) continue;
                    float eval = func.Solve(pos.x, pos.z, pos.y);
                    if (pos.y <= eval)
                    {
                        landed = true;
                    }
                }

                renderTex.SetPixel(x, height-y-1, landed ? landColor*(1f-normDepth) : blankColor);

                //renderTex.SetPixel(x, y, new Color((x*1f)/width, (y*1f)/height, 0f));
            }
        }
        renderTex.Apply();
    }
}
