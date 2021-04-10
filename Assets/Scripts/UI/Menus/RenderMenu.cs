using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using System.IO;
using UnityEditor;

public class RenderMenu : MonoBehaviour
{
    [Header("Render Mode")]
    [SerializeField] Toggle cpuMode;
    [SerializeField] Toggle gpuMode;
    [SerializeField] TMP_InputField threadsField;

    [Header("Process info")]
    [SerializeField] Button restartBtn;
    [SerializeField] Button recordBtn;
    [SerializeField] TextMeshProUGUI finalResText;
    [SerializeField] TextMeshProUGUI finalDepthText;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI liveThreadsText;
    [SerializeField] TextMeshProUGUI queuedThreadsText;

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

    private bool mustRender = false;
    private IRenderingFacade renderingFacade;
    private IPostProcessFacade postProcessFacade;
    private IFuncFacade funcFacade;

    private string recordPath;
    private string currentPath;
    private int currentStep;

    private void Start()
    {
        recordPath = Application.persistentDataPath;
        renderingFacade = ServiceLocator.Instance.GetService<IRenderingFacade>();
        postProcessFacade = ServiceLocator.Instance.GetService<IPostProcessFacade>();
        funcFacade = ServiceLocator.Instance.GetService<IFuncFacade>();
        restartBtn.onClick.AddListener(()=>renderingFacade.RequestRender());
        recordBtn.onClick.AddListener(() => StartRecord());

        GetOriginalData();
        LinkData();

        mustRender = renderingFacade.IsRendering;
        renderInitTime = DateTime.Now;
        UpdateAllInfo();
    }

    private void GetOriginalData()
    {
        threadsField.text = renderingFacade.MaxThreads.ToString();
        renderInitTime = DateTime.Now;

        InitRenderMode();
        cpuMode.isOn = renderingFacade.IsUsingCPUMode;
        gpuMode.isOn = renderingFacade.IsUsingGPUMode;

        #if UNITY_WEBGL
                gpuMode.interactable = false;
                cpuMode.isOn = true;
        #endif
    }

    private void LinkData()
    {
        cpuMode.onValueChanged.AddListener((val) =>
        {
            if (val) renderingFacade.UseCPURenderMode();
        });
        gpuMode.onValueChanged.AddListener((val) =>
        {
            if (val) renderingFacade.UseGPURenderMode();
        });

        threadsField.onValueChanged.AddListener((val) =>
        {
            if (int.TryParse(val, out int v))
            {
                v = Math.Abs(v);
                renderingFacade.MaxThreads = v;
                threadsField.SetTextWithoutNotify(v.ToString());
            }
        });

        renderingFacade.onRenderStarted += () =>
        {
            mustRender = true;
            renderInitTime = DateTime.Now;
            UpdateAllInfo();
        };
        renderingFacade.onRenderFinished += () =>
        {
            mustRender = false;
            UpdateAllInfo();
        };
        renderingFacade.onCPUModeActivated += InitRenderMode;
        renderingFacade.onGPUModeActivated += InitRenderMode;
    }

    #region InitRenderMode

    private void InitRenderMode()
    {
        int lastLevelRes = renderingFacade.GetFinalResolution();
        imgSize = Mathf.RoundToInt(Mathf.Pow(2, renderingFacade.MaxLevel - 1));
        finalResText.text = $"{lastLevelRes}x{lastLevelRes}";
        finalDepthText.text = renderingFacade.GetFinalDepth().ToString();

        processTex = CreateTex();
        homogeneityTex = CreateTex();
        processImg.texture = processTex;
        processImg.color = Color.white;
        homogeneityImg.texture = homogeneityTex;
        homogeneityImg.color = Color.white;
        renderInitTime = DateTime.Now;
    }

    private Texture2D CreateTex()
    {   
        var tex = new Texture2D(imgSize, imgSize, TextureFormat.RGB24, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        return tex;
    }

    #endregion

    #region Update

    private void OnEnable()
    {
        StartCoroutine(UpdateCoroutine());
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            if(mustRender)
                UpdateAllInfo();
            yield return new WaitForSeconds(processTime);
        }
    }

    private void UpdateAllInfo()
    {
        IRenderState renderState = renderingFacade.GetRenderState();
        UpdateProcessInfo();
        UpdateProcessTex(0, 0, imgSize, imgSize, renderState);
        UpdateHomogeneityTex(0, 0, imgSize, imgSize, renderState);
        processTex.Apply();
        homogeneityTex.Apply();
    }

#endregion

    #region process

    private void UpdateProcessInfo()
    {
        timeText.text = "" + Mathf.RoundToInt((float)(DateTime.Now - renderInitTime).TotalSeconds*1000f)/1000f + "s";
        liveThreadsText.text = renderingFacade.LiveThreads.ToString();
        queuedThreadsText.text = renderingFacade.QueuedThreads.ToString();
    }

    private void UpdateProcessTex(int minX, int minY, int maxX, int maxY, IRenderState renderState)
    {

        if (renderState.IsRenderingChildren && renderState.Children != null)
        {
            int halfSize = (maxX - minX)/2;
            UpdateProcessTex(minX, minY, maxX - halfSize, maxY - halfSize, renderState.Children[0]);
            UpdateProcessTex(minX+ halfSize, minY, maxX, maxY - halfSize, renderState.Children[1]);
            UpdateProcessTex(minX, minY+ halfSize, maxX - halfSize, maxY, renderState.Children[2]);
            UpdateProcessTex(minX+ halfSize, minY+ halfSize, maxX, maxY, renderState.Children[3]);
        } else
        {
            Color col;
            if (renderState.IsTextureApplied) col = texAppliedColor;
            else if (renderState.IsDone) col = doneColor;
            else if (renderState.IsQueued) col = queuedColor;
            else col = renderingColor;

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

    private void UpdateHomogeneityTex(int minX, int minY, int maxX, int maxY, IRenderState renderState)
    {

        if (renderState.IsRenderingChildren && renderState.Children != null)
        {
            int halfSize = (maxX - minX) / 2;
            UpdateHomogeneityTex(minX, minY, maxX - halfSize, maxY - halfSize, renderState.Children[0]);
            UpdateHomogeneityTex(minX + halfSize, minY, maxX, maxY - halfSize, renderState.Children[1]);
            UpdateHomogeneityTex(minX, minY + halfSize, maxX - halfSize, maxY, renderState.Children[2]);
            UpdateHomogeneityTex(minX + halfSize, minY + halfSize, maxX, maxY, renderState.Children[3]);
        }
        else
        {
            Color col = Color.Lerp(minHomogeneityColor, maxHomogeneityColor, Mathf.Pow(renderState.Homogeneity, 4f));

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


    #region Record

    private void StartRecord()
    {
        recordBtn.interactable = false;
        currentStep = 0;
        string funcName = funcFacade.GetSelectedFuncName();
        currentPath = $"{recordPath}/{funcName}_{(renderingFacade.IsUsingCPUMode ? "CPU" : "GPU")}";
        Debug.Log("Begin record: " + currentPath);
        if (!Directory.Exists(currentPath))
            Directory.CreateDirectory(currentPath);
        
        renderingFacade.onRenderStarted += AddRecordCallbacks;
        renderingFacade.RequestRender();
    }

    private void AddRecordCallbacks()
    {
        renderingFacade.onQuadRendered += RegisterQuadInRecord;
        renderingFacade.onRenderFinished += EndRecord;
        renderingFacade.onRenderCancelled += EndRecord;
    }

    private void RegisterQuadInRecord()
    {
        DateTime init = DateTime.Now;
        double time = (init - renderInitTime).TotalSeconds;
        Debug.Log($"RegisterQuadInRecord {currentStep} time");
        string path = $"{currentPath}/{currentStep}";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        List<string> infoLines = new List<string>();
        infoLines.Add($"time;{time}");
        File.WriteAllLines($"{path}/info.txt", infoLines.ToArray());

        byte[] partialResult = postProcessFacade.GetDisplayTextureCopy().EncodeToJPG();
        File.WriteAllBytes($"{path}/partialResult.jpg", partialResult);

        UpdateAllInfo();
        mustRender = false;
        byte[] state = processTex.EncodeToJPG();
        File.WriteAllBytes($"{path}/state.jpg", state);
        byte[] homogeneity = homogeneityTex.EncodeToJPG();
        File.WriteAllBytes($"{path}/homogeneity.jpg", homogeneity);

        currentStep++;
        renderInitTime +=(DateTime.Now - init);
    }

    private void EndRecord()
    {
        Debug.Log("End record");
        recordBtn.interactable = true;
        renderingFacade.onQuadRendered -= RegisterQuadInRecord;
        renderingFacade.onRenderFinished -= EndRecord;
        renderingFacade.onRenderCancelled -= EndRecord;
        renderingFacade.onRenderStarted -= AddRecordCallbacks;

        //System.Diagnostics.Process.Start("explorer.exe", "/select," + currentPath);
        //EditorUtility.RevealInFinder(currentPath);
        Application.OpenURL(currentPath);
    }

    #endregion
}
