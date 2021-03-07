﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using System.Collections.Concurrent;
using System.Threading;

public class RendererManager : MonoBehaviour
{
    public const bool DEBUG = false;

    [Header("Prototypes")]
    public static Renderer rootRenderer = null;
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
    public static Queue<Action> orders = new Queue<Action>();
    [SerializeField] private int ordersPerUpdate = 3;
    [SerializeField] private int maxParallelThreads = 10;

    [Header("Render settings")]
    [SerializeField] int explorationSamples = 10;
    [SerializeField] float depthExplorationMultiplier = 1.05f;
    [SerializeField] float normalExplorationMultiplier = 1.05f;
    [SerializeField] float normalPlaneMultiplier = 1.05f;

    //events
    public static event Action renderStarted;
    public static event Action renderFinished;
    private static bool rendering = false;

    private void Start()
    {
        Renderer.explorationSamples = explorationSamples;
        Renderer.depthExplorationMultiplier = depthExplorationMultiplier;
        Renderer.normalExplorationMultiplier = normalExplorationMultiplier;
        Renderer.normalPlaneMultiplier = normalPlaneMultiplier;
        maxLevel = setting.Count;
        texObjProto = localTexObjProto;
        cpuRendererProto = localCpuRendererProto;
        gpuRendererProto = localGpuRendererProto;
        spaceView = FindObjectOfType<ViewController>().GetComponent<RectTransform>();
        quadContainer = GameObject.FindGameObjectWithTag("QuadContainer").GetComponent<RectTransform>();
        rootRenderer = Instantiate(setting[0].type == RendererType.CPU ? cpuRendererProto : gpuRendererProto, transform).GetComponent<Renderer>();
        rootRenderer.gameObject.name = "rootRenderer";

        rootRenderer.Init(0);
        AdjustPositions();
        StartRender();
        FunctionPanel.onChanged += StartRender;
        ViewController.onChanged += StartRender;
    }

    #region ordersAndThreads

    private void FixedUpdate()
    {
        AttendOrders();
        AttendThreads();
    }

    private void AttendOrders()
    {
        int top = orders.Count;
        if (top > ordersPerUpdate) top = ordersPerUpdate;

        for (int i = 0; i < top; i++)
        {
            orders.Dequeue().Invoke();
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

    private void LateUpdate()
    {
        AdjustPositions();
        CheckIfRenderFinished();
    }

    private void AdjustPositions()
    {
        //rootRenderer.texTransform.position = spaceView.position+new Vector3(-spaceView.sizeDelta.x*0.5f, spaceView.sizeDelta.y*0.5f, 0f);
        //rootRenderer.texTransform.sizeDelta = spaceView.sizeDelta;
        rootRenderer.AdjustChildrenPositions();
    }

    #region rendering

    public static void StartRender()
    {
        renderStarted?.Invoke();
        rendering = true;
        orders.Clear();
        threadsToStart.Clear();
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
        //new RendererQuality(RendererType.CPU, 256, 50)
        new RendererQuality(RendererType.CPU, 64, 32),
        new RendererQuality(RendererType.CPU, 64, 80),
        new RendererQuality(RendererType.CPU, 128, 200),
        new RendererQuality(RendererType.CPU, 128, 700),
    };

    #endregion
}
