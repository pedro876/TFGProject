using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class RenderPanel : MonoBehaviour
{
    [Header("Process info")]
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
    float renderTime = 0f;

    bool mustUpdate = false;

    private void Start()
    {
        imgSize = Mathf.RoundToInt(Mathf.Pow(2, RendererManager.setting.Count - 1));
        processTex = new Texture2D(imgSize, imgSize, TextureFormat.RGB24, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        processImg.texture = processTex;
        processImg.color = Color.white;

        RendererManager.renderStarted += () =>
        {
            renderTime = 0f;
            mustUpdate = true;
        };
        RendererManager.renderFinished += () =>
        {
            mustUpdate = false;
            UpdateProcessInfo();
            UpdateProcessTex(0, 0, imgSize, imgSize, RendererManager.rootRenderer);
            processTex.Apply();
        };
    }

    private void Update()
    {
        if (mustUpdate) renderTime += Time.deltaTime;
    }

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
                UpdateProcessInfo();
                UpdateProcessTex(0, 0, imgSize, imgSize, RendererManager.rootRenderer);
                processTex.Apply();
            }
            
            yield return new WaitForSeconds(processTime);
        }
    }

    private void UpdateProcessInfo()
    {
        timeText.text = "" + (Mathf.RoundToInt(renderTime * 1000f) / 1000f) + "s";
        liveThreadsText.text = "" + RendererManager.currentThreads.Count;
        queuedThreadsText.text = "" + RendererManager.threadsToStart.Count;
        queuedDisplaysText.text = "" + RendererManager.orders.Count;
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


}
