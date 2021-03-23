using System.Collections;
using UnityEngine;
using System;

public interface IRenderingFacade
{
    event Action onQuadRendered;
    event Action onRenderStarted;
    event Action onRenderFinished;
    event Action onCPUModeActivated;
    event Action onGPUModeActivated;
    void RequestRender(bool forced);
    bool IsRendering { get; }
    int MaxThreads { get; set; }
    int LiveThreads { get; }
    int QueuedThreads { get; }
    void DisplayDepth();
    void DisplayNormals();
    void UseCPURenderMode();
    void UseGPURenderMode();
    bool IsUsingGPUMode { get; }
    bool IsUsingCPUMode { get; }
    int MaxLevel { get; }
    int GetLevelResolution(int level);
    int GetLevelDepth(int level);
    int GetFinalDepth();
    int GetFinalResolution();
    bool UpdateInRealTime { get; set; }
    IRenderState GetRenderState();
}
