using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class RenderPanel : MonoBehaviour
{
    [Header("Process info")]
    [SerializeField] Button restartBtn;
    [SerializeField] TextMeshProUGUI finalResText;
    [SerializeField] TextMeshProUGUI finalDepthText;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI liveThreadsText;
    [SerializeField] TextMeshProUGUI queuedThreadsText;
    [SerializeField] TextMeshProUGUI queuedDisplaysText;

    [Header("Process display")]
    [SerializeField] Color queuedColor;
    [SerializeField] Color renderingColor;
    [SerializeField] Color doneColor;
    [SerializeField] Color texAppliedColor;
    [SerializeField] float processTime = 0.1f;
    [SerializeField] RawImage processImg;
    private Texture2D processTex;
    private int imgSize;
    private DateTime renderInitTime;

    [Header("Homogeneity")]
    [SerializeField] RawImage homogeneityImg;
    [SerializeField] Color minHomogeneityColor = Color.red;
    [SerializeField] Color maxHomogeneityColor = Color.black;
    private Texture2D homogeneityTex;

    bool mustUpdate = true;
    public static event Action onRestartRender;

    private void Awake()
    {
        restartBtn.onClick.AddListener(()=>onRestartRender?.Invoke());

        imgSize = Mathf.RoundToInt(Mathf.Pow(2, RendererManager.setting.Count - 1));
        int lastLevelRes = imgSize * RendererManager.setting[RendererManager.setting.Count - 1].width;
        finalResText.text = "" + lastLevelRes + "x" + lastLevelRes;
        finalDepthText.text = "" + RendererManager.setting[RendererManager.setting.Count - 1].depth;

        processTex = new Texture2D(imgSize, imgSize, TextureFormat.RGB24, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        homogeneityTex = new Texture2D(imgSize, imgSize, TextureFormat.RGB24, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        processImg.texture = processTex;
        processImg.color = Color.white;
        homogeneityImg.texture = homogeneityTex;
        homogeneityImg.color = Color.white;

        renderInitTime = DateTime.Now;
        RendererManager.renderStarted += () =>
        {
            renderInitTime = DateTime.Now;
            mustUpdate = true;
        };
        RendererManager.renderFinished += () =>
        {
            mustUpdate = false;
            UpdateAllInfo();
        };
    }

    #region Update

    private void OnEnable()
    {
        StartCoroutine(ProcessUpdateCoroutine());
    }

    IEnumerator ProcessUpdateCoroutine()
    {
        while (true)
        {
            if(RendererManager.rootRenderer != null && mustUpdate)
            {
                UpdateAllInfo();
            }
            
            yield return new WaitForSeconds(processTime);
        }
    }

    private void UpdateAllInfo()
    {
        UpdateProcessInfo();
        UpdateProcessTex(0, 0, imgSize, imgSize, RendererManager.rootRenderer);
        UpdateHomogeneityTex(0, 0, imgSize, imgSize, RendererManager.rootRenderer);
        processTex.Apply();
        homogeneityTex.Apply();
    }

    #endregion

    #region process

    private void UpdateProcessInfo()
    {
        timeText.text = "" + Mathf.RoundToInt((float)(DateTime.Now - renderInitTime).TotalSeconds*1000f)/1000f + "s";
        liveThreadsText.text = "" + RendererManager.currentThreads.Count;
        queuedThreadsText.text = "" + RendererManager.threadsToStart.Count;
        queuedDisplaysText.text = "" + RendererManager.displayOrders.Count;
    }

    private void UpdateProcessTex(int minX, int minY, int maxX, int maxY, Renderer renderer)
    {

        if (renderer.IsRenderingChildren && renderer.children != null)
        {
            int halfSize = (maxX - minX)/2;
            UpdateProcessTex(minX, minY, maxX - halfSize, maxY - halfSize, renderer.children[0]);
            UpdateProcessTex(minX+ halfSize, minY, maxX, maxY - halfSize, renderer.children[1]);
            UpdateProcessTex(minX, minY+ halfSize, maxX - halfSize, maxY, renderer.children[2]);
            UpdateProcessTex(minX+ halfSize, minY+ halfSize, maxX, maxY, renderer.children[3]);
        } else
        {
            Color col;
            if (renderer.IsTextureApplied) col = texAppliedColor;
            else if (renderer.IsDone) col = doneColor;
            else if (renderer.IsRendering) col = renderingColor;
            else if (renderer.IsQueued) col = queuedColor;
            else col = Color.magenta;

            for(int x = minX; x < maxX; x++)
            {
                for(int y = minY; y < maxY; y++)
                {
                    processTex.SetPixel(x, processTex.height-y-1, col);
                }
            }
        }
    }

    #endregion

    #region Homogeneity

    private void UpdateHomogeneityTex(int minX, int minY, int maxX, int maxY, Renderer renderer)
    {

        if (renderer.IsRenderingChildren && renderer.children != null)
        {
            int halfSize = (maxX - minX) / 2;
            UpdateHomogeneityTex(minX, minY, maxX - halfSize, maxY - halfSize, renderer.children[0]);
            UpdateHomogeneityTex(minX + halfSize, minY, maxX, maxY - halfSize, renderer.children[1]);
            UpdateHomogeneityTex(minX, minY + halfSize, maxX - halfSize, maxY, renderer.children[2]);
            UpdateHomogeneityTex(minX + halfSize, minY + halfSize, maxX, maxY, renderer.children[3]);
        }
        else
        {
            Color col = Color.Lerp(minHomogeneityColor, maxHomogeneityColor, Mathf.Pow(renderer.GetHomogeneity(), 4f));

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    homogeneityTex.SetPixel(x, homogeneityTex.height - y - 1, col);
                }
            }
        }
    }

    #endregion

}
