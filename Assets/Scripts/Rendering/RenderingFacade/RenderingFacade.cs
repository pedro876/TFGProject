using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


namespace RenderingSpace
{
    public class RenderingFacade : MonoBehaviour, IRenderingFacade
    {
        public static RenderingFacade Instance => instance;
        private static RenderingFacade instance;

        public bool UpdateInRealTime { get; set; }
        public bool IsRendering { get; private set; }
        public int QuadTreeDepth => setting.Length;

        //Events
        public event Action onQuadRendered;
        public event Action onRenderStarted;
        public event Action onRenderFinished;
        public event Action onCPUModeActivated;
        public event Action onGPUModeActivated;

        //Threads and orders
        public object liveThreadsLock = new object();
        public Queue<Action> displayOrders = new Queue<Action>();
        public HashSet<Thread> liveThreads = new HashSet<Thread>();
        public List<KeyValuePair<Thread, int>> queuedThreads = new List<KeyValuePair<Thread, int>>();
        public List<KeyValuePair<Action, int>> renderOrders = new List<KeyValuePair<Action, int>>();

        private QuadLevel[] setting;
        private IRendererFactory factory;
        private IRenderer rootRenderer;
        private IFuncFacade funcFacade;
        private IViewFacade viewFacade;
        private IMassFacade massFacade;
        private IRegionFacade regionFacade;
        private RenderState renderState;

        private bool renderRequested = false;

        private void Awake()
        {
            instance = this;
            factory = new RendererFactory();
        }

        private void Start()
        {
            funcFacade = ServiceLocator.Instance.GetService<IFuncFacade>();
            viewFacade = ServiceLocator.Instance.GetService<IViewFacade>();
            massFacade = ServiceLocator.Instance.GetService<IMassFacade>();
            regionFacade = ServiceLocator.Instance.GetService<IRegionFacade>();
            
            //viewFacade.onChanged += () => RequestRender();
            massFacade.onChanged += () => RequestRender();
            regionFacade.onChanged += () => RequestRender();

            #if UNITY_WEBGL
                UseCPURenderMode();
            #else
                UseGPURenderMode();
            #endif

            StartCoroutine(AttendRenderOrders());
            StartCoroutine(RenderInterval());

            funcFacade.onChanged += () => RequestRender();
        }

        private void Update()
        {
            AttendOrders();
            AttendThreads();
            rootRenderer.Update();
            CheckIfRenderFinished();
            Render();
        }

        private void OnDestroy()
        {
            rootRenderer.Destroy();
        }

        #region RenderState

        public IRenderState GetRenderState()
        {
            renderState.ExtractStateFromRenderer(rootRenderer);
            return renderState;
        }

        #endregion

        #region Render

        public void TextureHasBeenApplied()
        {
            onQuadRendered?.Invoke();
        }

        public void RequestRender()
        {
            renderRequested = true;
        }

        private void Render()
        {
            if (renderRequested)
            {
                renderRequested = false;
                StopRender();
                IsRendering = true;
                onRenderStarted?.Invoke();
                rootRenderer.Render();
            }
        }

        IEnumerator RenderInterval()
        {
            while (true)
            {
                if(UpdateInRealTime)
                    RequestRender();
                yield return new WaitForSeconds(1f / RenderConfig.targetFramerate);
            }
        }

        private void CheckIfRenderFinished()
        {
            if(IsRendering && rootRenderer.IsDeepFinished)
            {
                IsRendering = false;
                onRenderFinished?.Invoke();
            }
        }

        

        #endregion

        #region Stop

        private void StopRender()
        {
            displayOrders.Clear();
            queuedThreads.Clear();
            renderOrders.Clear();
            AbortThreads();
            rootRenderer?.Stop();
        }

        private void AbortThreads()
        {
            lock (liveThreadsLock)
            {
                foreach (var t in liveThreads)
                {
                    if (t.IsAlive) t.Abort();
                }
                liveThreads.Clear();
            }
        }

        #endregion

        #region AttendOrders

        IEnumerator AttendRenderOrders()
        {
            while (true)
            {
                int count = renderOrders.Count;
                if (count > 0)
                {
                    renderOrders.Sort((a, b) => a.Value - b.Value);
                    Action action = renderOrders[count - 1].Key;
                    renderOrders.RemoveAt(count - 1);
                    action();
                }
                yield return new WaitForSeconds(RenderConfig.renderOrdersInterval);
            }
        }

        private void AttendOrders()
        {
            int top = displayOrders.Count;
            if (top > RenderConfig.displayOrdersPerFrame)
                top = RenderConfig.displayOrdersPerFrame;

            for (int i = 0; i < top; i++)
            {
                displayOrders.Dequeue().Invoke();
            }
        }

        #endregion

        #region Threading
        public int MaxThreads { get => RenderConfig.maxParallelThreads; set => RenderConfig.maxParallelThreads = value; }
        public int LiveThreads => liveThreads.Count;
        public int QueuedThreads => queuedThreads.Count;

        private void AttendThreads()
        {
            int threadsToStartCount = queuedThreads.Count;
            int top = GetAmountOfThreadsToRun(threadsToStartCount);

            queuedThreads.Sort((a, b) => a.Value - b.Value);

            for (int i = 0; i < top; i++)
            {
                Thread thread = queuedThreads[threadsToStartCount - i - 1].Key;
                queuedThreads.RemoveAt(threadsToStartCount - i - 1);
                lock (liveThreadsLock)
                {
                    liveThreads.Add(thread);
                }
                thread.Start();
            }
        }

        private int GetAmountOfThreadsToRun(int threadsToStartCount)
        {
            int top;
            int currentCount;
            lock (liveThreadsLock)
            {
                currentCount = liveThreads.Count;
            }
            top = RenderConfig.maxParallelThreads - currentCount;

            if (top > threadsToStartCount) top = threadsToStartCount;
            return top;
        }

        #endregion

        #region Display

        public void DisplayDepth()
        {
            rootRenderer.DisplayDepth();
        }

        public void DisplayNormals()
        {
            rootRenderer.DisplayNormals();
        }

        #endregion

        #region RenderMode

        public void UseCPURenderMode()
        {
            setting = RenderConfig.cpuSetting;
            rootRenderer?.Destroy();
            rootRenderer = factory.CreateCPURenderer();
            renderState = new RenderState(setting);
            RequestRender();
            onCPUModeActivated?.Invoke();
        }
        public void UseGPURenderMode()
        {
            setting = RenderConfig.gpuSetting;
            rootRenderer?.Destroy();
            rootRenderer = factory.CreateGPURenderer();
            renderState = new RenderState(setting);
            //RequestRender();
            onGPUModeActivated?.Invoke();
        }

        public bool IsUsingGPUMode => setting == RenderConfig.gpuSetting;

        public bool IsUsingCPUMode => setting == RenderConfig.cpuSetting;

        #endregion

        #region Levels

        public int MaxLevel => setting.Length;

        public int GetLevelResolution(int level)
        {
            level = ClampLevel(level);
            return setting[level].resolution;
        }

        public int GetLevelDepth(int level)
        {
            level = ClampLevel(level);
            return setting[level].depthSamples;
        }

        private int ClampLevel(int level)
        {
            if (level < 0) level = 0;
            if (level >= setting.Length) level = setting.Length - 1;
            return level;
        }

        public int GetFinalDepth()
        {
            return setting[setting.Length - 1].depthSamples;
        }

        public int GetFinalResolution()
        {
            return setting[setting.Length - 1].resolution * (int)Math.Pow(2, setting.Length-1);
        }

        #endregion
    }
}

