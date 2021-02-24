using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RendererManager : MonoBehaviour
{
    Renderer rootRenderer;
    [SerializeField] GameObject localTexObjProto;
    [SerializeField] GameObject localCpuRendererProto;
    [SerializeField] GameObject localGpuRendererProto;
    public static GameObject cpuRendererProto;
    public static GameObject gpuRendererProto;
    public static GameObject texObjProto;
    public static RectTransform spaceView;
    public static RectTransform functionViewContainer;

    public const bool DEBUG = false;

    public static Queue<Action> orders = new Queue<Action>();
    private const int ordersPerFrame = 5;

    private void Start()
    {
        texObjProto = localTexObjProto;
        cpuRendererProto = localCpuRendererProto;
        gpuRendererProto = localGpuRendererProto;
        spaceView = FindObjectOfType<ViewController>().GetComponent<RectTransform>();
        functionViewContainer = GameObject.FindGameObjectWithTag("FuncContainer").GetComponent<RectTransform>();
        rootRenderer = Instantiate(setting[0].type == RendererType.CPU ? cpuRendererProto : gpuRendererProto, transform).GetComponent<Renderer>();
        rootRenderer.gameObject.name = "rootRenderer";

        rootRenderer.Init(0);
        AdjustPositions();
        FunctionPanel.onChanged += StartRender;
        ViewController.onChanged += StartRender;
        StartRender();
    }

    private void FixedUpdate()
    {
        int top = orders.Count;
        if (top > ordersPerFrame) top = ordersPerFrame;

        for(int i = 0; i < top; i++)
        {
            orders.Dequeue().Invoke();
        }
    }

    private void LateUpdate()
    {
        AdjustPositions();
    }

    private void AdjustPositions()
    {
        rootRenderer.texTransform.position = spaceView.position+new Vector3(-spaceView.sizeDelta.x*0.5f, spaceView.sizeDelta.y*0.5f, 0f);
        rootRenderer.texTransform.sizeDelta = spaceView.sizeDelta;
        rootRenderer.AdjustChildrenPositions();
    }

    #region rendering

    private void StartRender()
    {
        orders.Clear();
        rootRenderer.DeepStop();
        rootRenderer.Render();
    }



    #endregion

    #region Quality

    public enum RendererType { CPU, GPU }

    public class RendererQuality
    {
        public readonly RendererType type;
        public readonly int width;
        public readonly int height;
        public readonly int depth;

        public RendererQuality(RendererType type, int size, int depth)
        {
            this.type = type;
            this.width = size;
            this.height = size;
            this.depth = depth;
        }
    }

    public static List<RendererQuality> setting = new List<RendererQuality>()
    {
        new RendererQuality(RendererType.CPU, 90, 40),
        new RendererQuality(RendererType.CPU, 64, 90),
        new RendererQuality(RendererType.CPU, 128, 300),
    };

    #endregion
}
