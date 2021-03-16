using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using System.Collections.Concurrent;
using System.Threading;

public class RendererManager : MonoBehaviour
{
    public const bool DEBUG = false;

    [Header("Prototypes")]
    public static AbstractRenderer rootRenderer = null;
    [SerializeField] GameObject localTexObjProto;
    [SerializeField] GameObject localCpuRendererProto;
    [SerializeField] GameObject localGpuRendererProto;
    public static GameObject cpuRendererProto;
    public static GameObject gpuRendererProto;
    public static GameObject texObjProto;
    public static RectTransform spaceView;
    public static RectTransform quadContainer;
    public static int maxLevel;

    [Header("Threading")]
    public static object currentThreadsLock = new object();
    public static HashSet<Thread> currentThreads = new HashSet<Thread>();
    public static List<KeyValuePair<Thread, int>> threadsToStart = new List<KeyValuePair<Thread, int>>();
    public static Queue<Action> displayOrders = new Queue<Action>();
    public static List<KeyValuePair<Action, int>> renderOrders = new List<KeyValuePair<Action, int>>();
    [SerializeField] private int displayOrdersPerFrame = 3;
    [SerializeField] private float renderOrdersInterval = 0.1f;
    [SerializeField] public int maxParallelThreads = 10;

    [Header("Render settings")]
    [SerializeField] float targetFramerate = 30f;
    [SerializeField] int gpuHomogeneityDepth = 256;
    [SerializeField] int explorationSamples = 10;
    [SerializeField] float depthExplorationMultiplier = 1.05f;
    [SerializeField] float normalExplorationMultiplier = 1.05f;
    [SerializeField] float normalPlaneMultiplier = 1.05f;
    private float renderInterval = 1000f;
    public RendererType renderMode { get; private set; }
    private bool CanRender { get => renderInterval > (1f / targetFramerate); }

    //events
    public static event Action renderStarted;
    public static event Action renderFinished;
    public static event Action<RendererManager.RendererType> onRenderModeChanged;
    public static bool rendering = false;

    public static RendererManager Instance { get; private set; }

    private void Start()
    {
        Instance = this;
        GPURenderer.homogeneityDepth = gpuHomogeneityDepth;
        AbstractRenderer.explorationSamples = explorationSamples;
        AbstractRenderer.depthExplorationMultiplier = depthExplorationMultiplier;
        AbstractRenderer.normalExplorationMultiplier = normalExplorationMultiplier;
        AbstractRenderer.normalPlaneMultiplier = normalPlaneMultiplier;
        
        texObjProto = localTexObjProto;
        cpuRendererProto = localCpuRendererProto;
        gpuRendererProto = localGpuRendererProto;
        spaceView = FindObjectOfType<ViewController>().GetComponent<RectTransform>();
        quadContainer = GameObject.FindGameObjectWithTag("QuadContainer").GetComponent<RectTransform>();
#if UNITY_WEBGL
        ChangeRenderMode(RendererType.CPU);
#else
        ChangeRenderMode(RendererType.GPU);
#endif
        //StartRender();
        ViewController.onChanged += () => StartRender(false);
        FunctionPanel.onChanged += () => StartRender(true);
        VolumeInterpreter.onChanged += () => StartRender(true);
        RenderPanel.onRestartRender += () => StartRender(true);
        StartCoroutine(AttendRenderOrders());
    }

    public static void DisplayDepth()
    {
        rootRenderer.DisplayDepth();
    }

    public static void DisplayNormal()
    {
        rootRenderer.DisplayNormal();
    }

#region ordersAndThreads

    bool mustAttendRenderOrder = false;
    IEnumerator AttendRenderOrders()
    {
        while (true)
        {
            mustAttendRenderOrder = true;
            yield return new WaitForSeconds(renderOrdersInterval);
        }
    }

    private void AttendOrders()
    {
        int top = displayOrders.Count;
        if (top > displayOrdersPerFrame) top = displayOrdersPerFrame;

        for (int i = 0; i < top; i++)
        {
            displayOrders.Dequeue().Invoke();
        }

        if (mustAttendRenderOrder)
        {
            mustAttendRenderOrder = false;
            int count = renderOrders.Count;
            if (count > 0)
            {
                renderOrders.Sort((a, b) => a.Value - b.Value);
                Action action = renderOrders[count - 1].Key;
                renderOrders.RemoveAt(count - 1);
                action();
            }
        }
    }

    private void AttendThreads()
    {
        int toStartCount = threadsToStart.Count;
        

        int top;
        int currentCount;
        lock (currentThreadsLock)
        {
            currentCount = currentThreads.Count;
            //Debug.Log(currentCount);
        }
        top = maxParallelThreads - currentCount;
        
        if (top > toStartCount) top = toStartCount;
        
        threadsToStart.Sort((a, b) =>  a.Value - b.Value);

        for (int i = 0; i < top; i++)
        {
            Thread thread = threadsToStart[toStartCount - i - 1].Key;
            //Debug.Log(threadsToStart[toStartCount - i - 1].Value);
            threadsToStart.RemoveAt(toStartCount - i - 1);

            //Thread thread = threadsToStart.Dequeue();
            lock (currentThreadsLock)
            {
                currentThreads.Add(thread);
            }
            thread.Start();
        }
    }

#endregion

    private void Update()
    {
        if (renderInterval < (1f / targetFramerate)) renderInterval += Time.deltaTime;
        AttendOrders();
        AttendThreads();
        AdjustPositions();
        CheckIfRenderFinished();
    }

    private void AdjustPositions()
    {
        rootRenderer.AdjustChildrenPositions();
    }

#region rendering

    public void StartRender(bool forced = false)
    {
        if (!CanRender && !forced) return;
        while (renderInterval > (1f / targetFramerate)) renderInterval -= (1f / targetFramerate);
        //renderInterval = 0f;
        renderStarted?.Invoke();
        rendering = true;
        displayOrders.Clear();
        threadsToStart.Clear();
        renderOrders.Clear();
        lock (currentThreadsLock)
        {
            foreach(var t in currentThreads)
            {
                if (t.IsAlive) t.Abort();
            }
            currentThreads.Clear();
        }
        
        rootRenderer.DeepStop();
        rootRenderer.Render();
        return;
    }

    private void CheckIfRenderFinished()
    {
        if(rendering && rootRenderer != null && rootRenderer.deepFinished/* && orders.Count == 0*/)
        {
            rendering = false;
            renderFinished?.Invoke();
        }
    }

#endregion

#region Quality

    public void ChangeRenderMode(RendererType type)
    {
        renderMode = type;
        setting = type == RendererType.CPU ? CPUSetting : GPUSetting;
        maxLevel = setting.Count;
        if (rootRenderer != null)
        {
            DestroyImmediate(rootRenderer.gameObject);
        }
        rootRenderer = Instantiate(setting[0].type == RendererType.CPU ? cpuRendererProto : gpuRendererProto, transform).GetComponent<AbstractRenderer>();
        rootRenderer.gameObject.name = "rootRenderer";
        rootRenderer.Init(0);
        AdjustPositions();
        
        onRenderModeChanged?.Invoke(type);
        StartRender();
    }

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

    public static List<RendererQuality> setting;

    private static List<RendererQuality> GPUSetting = new List<RendererQuality>()
    {
        new RendererQuality(RendererType.GPU, 256, 128),
        new RendererQuality(RendererType.GPU, 176, 128),
        new RendererQuality(RendererType.GPU, 128, 200),
        new RendererQuality(RendererType.GPU, 176, 700),
    };

    private static List<RendererQuality> CPUSetting = new List<RendererQuality>()
    {
        new RendererQuality(RendererType.CPU, 64, 90),
        new RendererQuality(RendererType.CPU, 128, 128),
        new RendererQuality(RendererType.CPU, 128, 200),
        new RendererQuality(RendererType.CPU, 128, 700),
    };

#endregion
}
